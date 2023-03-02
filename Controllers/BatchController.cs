using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AssessmentAPI.Models;
using System;
using System.Linq;
using AssessmentAPI.Service;
using AssessmentAPI.Validators;
using FluentValidation;
using AssessmentAPI.Logger;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using AssessmentAPI.DataLayer;
using System.Net.NetworkInformation;
using System.ComponentModel.DataAnnotations;
using ValidationException = FluentValidation.ValidationException;
using System.ComponentModel;

namespace AssessmentAPI.Controllers
{
    [ApiController]
    public class BatchController : ControllerBase
    {
        #region Private Variables

        private readonly IBatchService _batchService;
        private BatchValidator _validator;
        private BatchDetailsResponseValidator _batchDetailsValidator;
        private BatchResponseValidator _batchResponseValidator;
        private readonly ILogger _logger;
        private string _correlationId;
        private ErrorResponse _errorResponse;
        #endregion

        #region Constructor
        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="batchService">Batch Service</param>
        /// <param name="logger">Logger service</param>
        public BatchController(IBatchService batchService, ILogger logger)
        {
            _batchService = batchService;
            _logger = logger;
            _validator = new BatchValidator();
            _batchDetailsValidator = new BatchDetailsResponseValidator();
            _batchResponseValidator = new BatchResponseValidator();
            _errorResponse = new ErrorResponse();
        }

        #endregion

        #region Pulic Methods

        /// <summary>
        /// Create a new batch to upload files into
        /// </summary>
        /// <param name="batchRequest">Request Body</param>
        /// <returns>Returns the batch id</returns>
        /// <response code="401">Unauthorised - either you have not provided any credentials, or your credentials are not recognised</response>
        /// <response code="403">Forbidden - you have been authorised, but you are not allowed to access this resource</response>
        /// <response code="400">Bad Request - there are one or more errors in the specified parameters</response>

        [HttpPost]
        [Route("/batch")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BatchResponse), StatusCodes.Status201Created)]
        [Produces("application/json")]
        public IActionResult Batch([FromBody] BatchRequest batchRequest)
        {
            try
            {
                _logger.LogInfo($"Execution of {nameof(BatchController)}.{nameof(Batch)} method begin",
                    ("Request object", JsonConvert.SerializeObject(batchRequest))
                    );
                _validator.ValidateAndThrow(batchRequest);  

                if (!_batchService.ValidateBusinessUnit(batchRequest.BusinessUnit))
                {
                    _correlationId = _logger.LogWarning($"BusinessUnit not found",
                        ("Business Unit Name", batchRequest.BusinessUnit),
                        ("Method Name", $"{nameof(BatchController)}.{nameof(Batch)}"),
                        ("Status Code", StatusCodes.Status400BadRequest.ToString())
                        );
                    _errorResponse.Errors = new ErrorResponse.Error[]
                         {
                         new ErrorResponse.Error() {
                             Source = $"{nameof(Batch)}",
                             Description = BatchConstants.BUSINESS_UNIT_INVALID
                         }
                        };
                    _errorResponse.CorrelationId = _correlationId;
                    return BadRequest(_errorResponse);
                }

                if (batchRequest.Attritubes != null)
                {
                    if (batchRequest.Attritubes.Length <= 0)
                    {
                        _correlationId = _logger.LogWarning($"Attribute should not be empty",
                        ("Method Name", $"{nameof(BatchController)}.{nameof(Batch)}"),
                        ("Status Code", StatusCodes.Status400BadRequest.ToString())
                        );
                        _errorResponse.Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source = $"{nameof(Batch)}",
                                    Description = BatchConstants.ATTRIBUTE_VALIDATION
                                }
                            };
                        _errorResponse.CorrelationId = _correlationId;
                        return BadRequest(_errorResponse);
                    }

                    if (batchRequest.Attritubes.Any(i => string.IsNullOrWhiteSpace(i.Key) || string.IsNullOrWhiteSpace(i.Value)))
                    {
                        _correlationId = _logger.LogWarning($"Attribute should contain both key and value",
                            ("Method Name", $"{nameof(BatchController)}.{nameof(Batch)}"),
                            ("Status Code", StatusCodes.Status400BadRequest.ToString())
                                );
                        _errorResponse.Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source = $"{nameof(Batch)}",
                                    Description = BatchConstants.ATTRIBUTE_VALIDATION_KEY_VALUE
                                }
                            };
                        _errorResponse.CorrelationId = _correlationId;

                        return BadRequest(_errorResponse);
                    }
                }

                var _guid = _batchService.CreateBatch(batchRequest);
                var _batchResponse = new BatchResponse() { BatchId = _guid };
                _batchResponseValidator.ValidateAndThrow(_batchResponse);
                _batchService.CreateStorageContainer(_guid.ToString());
                _logger.LogInfo($"Execution of {nameof(BatchController)}.{nameof(Batch)} method end",
                    ("Response object", JsonConvert.SerializeObject(_batchResponse)
                    ));

                return Created("batch", _batchResponse);
            }
            catch (ValidationException ex)
            {
                _correlationId = _logger.LogWarning(ex.Message,
                    ("Method Name", $"{nameof(BatchController)}.{nameof(Batch)}"),
                    ("Description", ex.Message),
                    ("Source", ex.Source),
                    ("Status Code", StatusCodes.Status400BadRequest.ToString())
                    );

                _errorResponse.Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source =ex.Source,
                                    Description = ex.Message
                                }
                            };
                _errorResponse.CorrelationId = _correlationId;
                return BadRequest(_errorResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured in {nameof(BatchController)}.{nameof(Batch)}",
                     ("Request object", JsonConvert.SerializeObject(batchRequest))
                     );
                throw ex;
            }
        }

        /// <summary>
        /// Get details of the batch including links to all the files in the batch
        /// </summary>
        /// <remarks>This get will include full details of the batch, for example it's status, the files in the batch.</remarks>
        /// <param name="batchId">A Batch ID</param>
        /// <returns>Returns details about the batch</returns>
        /// <response code="200">OK - Return details about the batch</response>
        /// <response code="401">Unauthorised - either you have not provided any credentials, or your credentials are not recognised</response>
        /// <response code="403">Forbidden - you have been authorised, but you are not allowed to access this resource</response>
        /// <response code="404">Not Found - Could be that the batch ID doesn't exist</response>
        /// <response code="410">Gone - the batch has been expired and is no longer available</response>
        /// <response code="400">Bad Request - could be a invalid batch Id format. Batch IDs should be a GUID. A valid GUID that doesn't match a batch ID will return a 404</response>

        [HttpGet]
        [Route("/batch/{batchId}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BatchDetailsResponse), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public IActionResult GetBatchDetails(Guid batchId)
        {
            try
            {
                _logger.LogInfo($"Execution of {nameof(BatchController)}.{nameof(GetBatchDetails)} method begin",
                    ("Batch Id", batchId.ToString())
                    );
                var _status = _batchService.ValidateBatchID(batchId);

                if (_status == StatusCodes.Status404NotFound)
                {
                    _logger.LogWarning($"Batch not found",
                        ("Batch Id", batchId.ToString()),
                        ("Method Name", $"{nameof(BatchController)}.{nameof(GetBatchDetails)}"),
                        ("Status Code", _status.ToString())
                        );
                    return NotFound();
                }
                else if (_status == StatusCodes.Status410Gone)
                {
                    _logger.LogWarning($"Batch is expired",
                        ("Batch Id", batchId.ToString()),
                        ("Method Name", $"{nameof(BatchController)}.{nameof(GetBatchDetails)}")
                        );
                    return StatusCode(StatusCodes.Status410Gone);
                }

                var _batchDetails = _batchService.GetBatchDetails(batchId);
                _batchDetailsValidator.ValidateAndThrow(_batchDetails);
                _logger.LogInfo($"Execution of {nameof(BatchController)}.{nameof(GetBatchDetails)} method end",
                     ("Batch Id", batchId.ToString()),
                     ("Response object", JsonConvert.SerializeObject(_batchDetails))
                     );
                return Ok(_batchDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured in {nameof(BatchController)}.{nameof(GetBatchDetails)}",
                    ("Batch Id", batchId.ToString())
                    );
                throw ex;
            }
        }

        /// <summary>
        /// Add a file to the batch
        /// </summary>
        /// <remarks>Creates a file in the batch. To upload the content of the file, one or more <code>uploadBlockOfFile</code> 
        /// requests will need to be made followed by a 'putBlocksInFile' request to complete the file.</remarks>
        /// <param name="batchId">A Batch ID</param>
        /// <param name="filename">Filename for the new file. Must be unique in the batch (but can be same as another file 
        /// in another batch). Filenames don't include a path.</param>
        /// <param name="X_MIME_Type">Optional. The MIME content type of the file. The default type is application/octet-stream</param>
        /// <param name="X_Content_Size">The final size of the file in bytes.</param>
        /// <returns></returns>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request - Could be a batch ID; a batch ID that doesn't exist; a bad filename</response>
        /// <response code="401">Unauthorised - either you have not provided any credentials, or your credentials are not recognised</response>
        /// <response code="403">Forbidden - you have been authorised, but you are not allowed to access this resource</response>
        [HttpPost]
        [Route("/batch/{batchId}/{filename}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [Produces("application/json")]
        public IActionResult AddFiles(Guid batchId,string filename, [FromHeader] string X_MIME_Type, [FromHeader][Required] double X_Content_Size)
        {
            try
            {
                _logger.LogInfo($"Execution of {nameof(BatchController)}.{nameof(AddFiles)} method begin",
                 ("Batch Id", batchId.ToString()),
                 ("File Name", filename),
                 ("X_MIME_Type", X_MIME_Type),
                 ("X_Content_Size", X_Content_Size.ToString())
                 );

                if(string.IsNullOrEmpty(filename) || !_batchService.IsValidFilename(filename))
                {
                    _correlationId = _logger.LogWarning("Invalid file name",
                        ("Batch ID", batchId.ToString()),
                        ("File Name", filename)
                        );
                    _errorResponse.Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source = $"{nameof(AddFiles)}",
                                    Description = BatchConstants.INVALID_FILE_NAME
                                }
                            };
                    _errorResponse.CorrelationId = _correlationId;
                    return BadRequest(_errorResponse);
                }
                var _status = _batchService.ValidateBatchID(batchId);

                if (_status == StatusCodes.Status404NotFound)
                {
                    _logger.LogWarning($"Batch not found",
                        ("Batch Id", batchId.ToString()),
                        ("Method Name", $"{nameof(BatchController)}.{nameof(GetBatchDetails)}"),
                        ("Status Code", _status.ToString())
                        );
                    return NotFound();
                }
                else if (_status == StatusCodes.Status410Gone)
                {
                    _logger.LogWarning($"Batch is expired",
                        ("Batch Id", batchId.ToString()),
                        ("Method Name", $"{nameof(BatchController)}.{nameof(GetBatchDetails)}")
                        );
                    return StatusCode(StatusCodes.Status410Gone);
                }

                if (string.IsNullOrEmpty(X_MIME_Type) || string.IsNullOrWhiteSpace(X_MIME_Type))
                {
                    X_MIME_Type = BatchConstants.APPLICATION_OCTET_STREAM;
                }

                if (_batchService.UploadFileToContainer(batchId.ToString(), filename))
                {
                    _batchService.AddFileDetails(batchId.ToString(), filename, X_MIME_Type, X_Content_Size);
                }
                else
                {
                    _correlationId = _logger.LogWarning("File already exist for this batch",
                        ("Batch ID", batchId.ToString()),
                        ("File Name", filename)
                        );
                    _errorResponse.Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source = $"{nameof(AddFiles)}",
                                    Description = BatchConstants.FILE_ALREADY_EXIST
                                }
                            };
                    _errorResponse.CorrelationId = _correlationId;
                    return BadRequest(_errorResponse);
                }

                _logger.LogInfo($"Execution of {nameof(BatchController)}.{nameof(AddFiles)} method end");
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured in {nameof(BatchController)}.{nameof(GetBatchDetails)}",
                    ("Batch Id", batchId.ToString()),
                    ("File Name", filename),
                    ("X_MIME_Type", X_MIME_Type),
                    ("X_Content_Size", X_Content_Size.ToString())
                    );
                throw ex;
            }
        }
        #endregion
    }
}
