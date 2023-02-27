using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using AssessmentAPI.Controllers;
using AssessmentAPI.Service;
using Microsoft.AspNetCore.Mvc;
using AssessmentAPI.Models;
using Microsoft.AspNetCore.Http;
using AssessmentAPI.Logger;
using System.Linq;
using Assert = NUnit.Framework.Assert;
using AssessmentAPI.DataLayer;
using FluentValidation;

namespace APITests
{
    internal class BatchTests
    {
        #region Private Variables
        BatchController _controller;
        IBatchService _fakeBatchService;
        ILogger _fakeLoggerService; 
        #endregion

        #region Constructor
        public BatchTests()
        {
            _fakeBatchService = A.Fake<IBatchService>();
            _fakeLoggerService = A.Fake<ILogger>();
            _controller = new BatchController(_fakeBatchService, _fakeLoggerService);

        } 
        #endregion

        #region Batch Method Tests

        [Test]
        public void Batch_ReturnsAGuid_WhenBatchRequestIsSent()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[]
                {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Key = "key2", Value = "value2"}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).Returns(_guid);

            //Act 
            var result = (CreatedResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened().Then(
                A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustHaveHappened());
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);
        }

        [Test]
        public void Batch_ReturnsBadRequest_WhenEmptyRequest()
        {
            //Arrange
            var _businessUnit = "IT";
            var batchRequest = new BatchRequest();
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);

            //Act 
            var result = (BadRequestObjectResult)_controller.Batch(batchRequest);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.IsInstanceOf(typeof(ErrorResponse), result.Value);
        }

        [Test]
        public void Batch_ReturnsBadRequest_WhenInvalidBusinessUnitIsPassed()
        {
            //Arrange
            var _businessUnit = "IT";
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[]
                {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Key = "key2", Value = "value2"}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(false);

            //Act 
            var result = (BadRequestObjectResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened();
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustNotHaveHappened();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf(typeof(ErrorResponse), result.Value);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual(BatchConstants.BUSINESS_UNIT_INVALID, ((ErrorResponse)result.Value).Errors.FirstOrDefault().Description);
        }

        [Test]
        public void Batch_ReturnsAGuid_WhenBatchRequestAttribbutesAreNull()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = null
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).Returns(_guid);

            //Act 
            var result = (CreatedResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened().Then(
                A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustHaveHappened());
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);
        }

        [Test]
        public void Batch_ReturnsBadRequest_WhenAttributtesKeyValueAreNull()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[] { }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).Returns(_guid);

            //Act 
            var result = (BadRequestObjectResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened();
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustNotHaveHappened();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf(typeof(ErrorResponse), result.Value);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual(BatchConstants.ATTRIBUTE_VALIDATION, ((ErrorResponse)result.Value).Errors.FirstOrDefault().Description);
        }

        [Test]
        public void Batch_ReturnsBadRequest_WhenAttributtesKeyIsEmpty()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[] {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Key = "", Value = "value2"}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);

            //Act 
            var result = (BadRequestObjectResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened();
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustNotHaveHappened();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf(typeof(ErrorResponse), result.Value);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual(BatchConstants.ATTRIBUTE_VALIDATION_KEY_VALUE, ((ErrorResponse)result.Value).Errors.FirstOrDefault().Description);
        }

        [Test]
        public void Batch_ReturnsBadRequest_WhenAttributtesValueIsEmpty()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[] {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Key = "key1", Value = ""}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);

            //Act 
            var result = (BadRequestObjectResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened();
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustNotHaveHappened();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf(typeof(ErrorResponse), result.Value);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual(BatchConstants.ATTRIBUTE_VALIDATION_KEY_VALUE, ((ErrorResponse)result.Value).Errors.FirstOrDefault().Description);
        }

        [Test]
        public void Batch_ReturnsBadRequest_WhenAttributtesValueIsNull()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[] {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Key = "key1"}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);

            //Act 
            var result = (BadRequestObjectResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened();
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustNotHaveHappened();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf(typeof(ErrorResponse), result.Value);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual(BatchConstants.ATTRIBUTE_VALIDATION_KEY_VALUE, ((ErrorResponse)result.Value).Errors.FirstOrDefault().Description);
        }

        [Test]
        public void Batch_ReturnsBadRequest_WhenAttributtesKeyIsNull()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[] {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Value = "value2"}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);

            //Act 
            var result = (BadRequestObjectResult)_controller.Batch(batchRequest);

            //Assert
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).MustHaveHappened();
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).MustNotHaveHappened();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf(typeof(ErrorResponse), result.Value);
            Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
            Assert.AreEqual(BatchConstants.ATTRIBUTE_VALIDATION_KEY_VALUE, ((ErrorResponse)result.Value).Errors.FirstOrDefault().Description);
        }

        [Test]
        public void Batch_ThrowsException_WhenCreateBatchThrowsException()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[]
                {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Key = "key2", Value = "value2"}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Returns(true);
            A.CallTo(() => _fakeBatchService.CreateBatch(batchRequest)).Throws(new NotSupportedException("Fake Exception"));

            //Act 
            var ex = Assert.Throws<NotSupportedException>(() =>
               _controller.Batch(batchRequest));

            //Assert
            Assert.IsNotNull(ex);
            Assert.That(ex.Message, Is.EqualTo("Fake Exception"));
        }

        [Test]
        public void Batch_ThrowsException_WhenValidateBusinessUnitThrowsException()
        {
            //Arrange
            var _businessUnit = "IT";
            var _guid = Guid.NewGuid();
            var batchRequest = new BatchRequest()
            {
                BusinessUnit = _businessUnit,
                ExpiryDate = DateTime.Now.AddDays(10),
                Acl1 = new BatchRequest.Acl()
                {
                    ReadGroups = new string[] { "Group 1", "Group 2" },
                    ReadUsers = new string[] { "User1", "User 2" }
                },
                Attritubes = new BatchRequest.Attritube[]
                {
                    new BatchRequest.Attritube() {Key  = "key1",Value = "value1"},
                    new BatchRequest.Attritube() {Key = "key2", Value = "value2"}
                }
            };
            A.CallTo(() => _fakeBatchService.ValidateBusinessUnit(_businessUnit)).Throws(new NotSupportedException("Fake Exception"));

            //Act 
            var ex = Assert.Throws<NotSupportedException>(() =>
               _controller.Batch(batchRequest));

            //Assert
            Assert.That(ex.Message, Is.EqualTo("Fake Exception"));
        }
        #endregion

        #region GetBatchDetails Method Tests

        [Test]
        public void GetBatchDetails_ReturnsBatchDetails_WhenValidBatchIdIsPassed()
        {
            //Arrange
            var _guid = Guid.NewGuid();
            var _batchResponse = new BatchDetailsResponse()
            {
                BatchId = _guid.ToString(),
                BusinessUnit = "IT",
                ExpiryDate = DateTime.Now.AddDays(10)
            };

            A.CallTo(() => _fakeBatchService.ValidateBatchID(_guid)).Returns(StatusCodes.Status200OK);
            A.CallTo(() => _fakeBatchService.GetBatchDetails(_guid)).Returns(_batchResponse);

            //Act
            var result = (OkObjectResult)_controller.GetBatchDetails(_guid);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOf(typeof(BatchDetailsResponse), result.Value);
            Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
            Assert.AreEqual(_batchResponse, (BatchDetailsResponse)result.Value);
        }

        [Test]
        public void GetBatchDetails_ReturnsNotFound_WhenInvalidBatchIdIsPassed()
        {
            //Arrange
            var _guid = Guid.NewGuid();
            var _batchResponse = new BatchDetailsResponse()
            {
                BatchId = _guid.ToString(),
                BusinessUnit = "IT",
                ExpiryDate = DateTime.Now.AddDays(10)
            };

            A.CallTo(() => _fakeBatchService.ValidateBatchID(_guid)).Returns(StatusCodes.Status404NotFound);

            //Act
            var result = (NotFoundResult)_controller.GetBatchDetails(_guid);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [Test]
        public void GetBatchDetails_ReturnsGone_WhenValidBatchIsExpired()
        {
            //Arrange
            var _guid = Guid.NewGuid();
            var _batchResponse = new BatchDetailsResponse()
            {
                BatchId = _guid.ToString(),
                BusinessUnit = "IT",
                ExpiryDate = DateTime.Now.AddDays(-10)
            };

            A.CallTo(() => _fakeBatchService.ValidateBatchID(_guid)).Returns(StatusCodes.Status410Gone);
            A.CallTo(() => _fakeBatchService.GetBatchDetails(_guid)).Returns(_batchResponse);

            //Act
            var result = (StatusCodeResult)_controller.GetBatchDetails(_guid);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status410Gone, result.StatusCode);
        }

        [Test]
        public void GetBatchDetails_ThrowsException_WhenValidBatchIsExpiredButGoneConditionIsTrue()
        {
            //Arrange
            var _guid = Guid.NewGuid();
            var _batchResponse = new BatchDetailsResponse()
            {
                BatchId = _guid.ToString(),
                BusinessUnit = "IT",
                ExpiryDate = DateTime.Now.AddDays(-10)
            };

            A.CallTo(() => _fakeBatchService.ValidateBatchID(_guid)).Returns(StatusCodes.Status200OK);
            A.CallTo(() => _fakeBatchService.GetBatchDetails(_guid)).Returns(_batchResponse);

            //Act
            var ex = Assert.Throws<ValidationException>(() =>
               _controller.GetBatchDetails(_guid));

            //Assert
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetBatchDetails_ThrowsException_WhenGetBatchDetailsThrowsExceptions()
        {
            //Arrange
            var _guid = Guid.NewGuid();
            var _batchResponse = new BatchDetailsResponse()
            {
                BatchId = _guid.ToString(),
                BusinessUnit = "IT",
                ExpiryDate = DateTime.Now.AddDays(10)
            };

            A.CallTo(() => _fakeBatchService.ValidateBatchID(_guid)).Returns(StatusCodes.Status200OK);
            A.CallTo(() => _fakeBatchService.GetBatchDetails(_guid)).Throws(new NotSupportedException("Fake Exception"));

            //Act
            var ex = Assert.Throws<NotSupportedException>(() =>
               _controller.GetBatchDetails(_guid));

            //Assert
            Assert.IsNotNull(ex);
            Assert.That(ex.Message, Is.EqualTo("Fake Exception"));
        }
        #endregion
    }
}
