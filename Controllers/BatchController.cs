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
                    return BadRequest(new ErrorResponse
                    {
                        CorrelationId = _correlationId,
                        Errors = new ErrorResponse.Error[]
                        {
                        new ErrorResponse.Error() {
                            Source = $"{nameof(Batch)}",
                            Description = BatchConstants.BUSINESS_UNIT_INVALID
                        }
                        }
                    });
                }

                if (batchRequest.Attritubes != null)
                {
                    if (batchRequest.Attritubes.Length <= 0)
                    {
                        _correlationId = _logger.LogWarning($"Attribute should not be empty",
                        ("Method Name", $"{nameof(BatchController)}.{nameof(Batch)}"),
                        ("Status Code", StatusCodes.Status400BadRequest.ToString())
                        );

                        return BadRequest(new ErrorResponse
                        {
                            CorrelationId = _correlationId,
                            Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source = $"{nameof(Batch)}",
                                    Description = BatchConstants.ATTRIBUTE_VALIDATION
                                }
                            }
                        }
                        );
                    }

                    if (batchRequest.Attritubes.Any(i => string.IsNullOrWhiteSpace(i.Key) || string.IsNullOrWhiteSpace(i.Value)))
                    {
                        _correlationId = _logger.LogWarning($"Attribute should contain both key and value",
                            ("Method Name", $"{nameof(BatchController)}.{nameof(Batch)}"),
                            ("Status Code", StatusCodes.Status400BadRequest.ToString())
                                );
                        return BadRequest(new ErrorResponse
                        {
                            CorrelationId = _correlationId,
                            Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source = $"{nameof(Batch)}",
                                    Description = BatchConstants.ATTRIBUTE_VALIDATION_KEY_VALUE
                                }
                            }
                        }
                        );
                    }
                }

                var _guid = _batchService.CreateBatch(batchRequest);
                var _batchResponse = new BatchResponse() { BatchId = _guid };
                _batchResponseValidator.ValidateAndThrow(_batchResponse);
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

                return BadRequest(new ErrorResponse
                {
                    CorrelationId = _correlationId,
                    Errors = new ErrorResponse.Error[]
                            {
                                new ErrorResponse.Error() {
                                    Source =ex.Source,
                                    Description = ex.Message
                                }
                            }
                });
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
        #endregion
    }
}
