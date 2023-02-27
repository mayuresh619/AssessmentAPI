using AssessmentAPI.DataLayer;
using AssessmentAPI.Logger;
using AssessmentAPI.Models;
using AssessmentAPI.Service;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Bson;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace APITests
{
    [TestFixture]
    internal class BatchServiceTests
    {
        #region Private Variables
        ILogger _fakeLoggerService;
        IConfiguration _config;
        BatchService _batchService;
        AssessmentDBContext _context;
        Guid _createBatchID;

        #endregion

        #region Constructor
        public BatchServiceTests()
        {
            _fakeLoggerService = A.Fake<ILogger>();
            _config = InitConfiguration();
            _context = new AssessmentDBContext(_config);
            _batchService = new BatchService(_config, _fakeLoggerService);
        }

        #endregion

        #region ValidateBusinessUnit Method Tests
        [Test]
        public void ValidateBusinessUnit_ReturnsFalse_WhenInvalidBusinessNameIsPassed()
        {
            //Arrange
            var _businessUnit = "Abc";

            //Act
            var result = _batchService.ValidateBusinessUnit(_businessUnit);

            //Assert
            Assert.IsFalse(result);

        }

        [Test]
        public void ValidateBusinessUnit_ReturnsFalse_WhenValidBusinessNameIsPassed()
        {
            //Arrange
            var _businessUnit = "IT";

            //Act
            var result = _batchService.ValidateBusinessUnit(_businessUnit);

            //Assert
            Assert.IsTrue(result);

        }

        #endregion

        #region GetBatchDetails Method Tests

        [Test]
        public void GetBatchDetails_ReturnsBatchDetails_WhenValidBatchIdIsPassed()
        {
            //Arrange
            var _batchID = Guid.Parse("27af1e01-ad1d-48df-946e-ec2b2332a4f1");
            var expectedBusinessUnit = "Security";

            //Act
            var result = _batchService.GetBatchDetails(_batchID);

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.BatchId);
            Assert.IsNotNull(result.BusinessUnit);
            Assert.AreEqual(_batchID.ToString(), result.BatchId);
            Assert.AreEqual(expectedBusinessUnit, result.BusinessUnit);
        }

        [Test]
        public void GetBatchDetails_ThrowsException_WhenInvalidBatchIdIsPassed()
        {
            //Arrange
            var _batchID = Guid.Parse("27af1e01-ad1d-48df-946e-ec2b2332a4f2");

            //Act
            var ex = Assert.Throws<InvalidOperationException>(() => _batchService.GetBatchDetails(_batchID));

            //Assert
            Assert.IsNotNull(ex);
        }

        #endregion

        #region ValidateBatchID Method Tests

        [Test]
        public void ValidateBatchID_Returns200Status_WhenValidBatchIdIsPassed()
        {
            //Arrange
            var _batchID = Guid.Parse("27af1e01-ad1d-48df-946e-ec2b2332a4f1");

            //Act
            var result = _batchService.ValidateBatchID(_batchID);

            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, result);
        }

        [Test]
        public void ValidateBatchID_Returns404Status_WhenInvalidBatchIdIsPassed()
        {
            //Arrange
            var _batchID = Guid.Parse("27af1e01-ad1d-48df-946e-ec2b2332a4f2");

            //Act
            var result = _batchService.ValidateBatchID(_batchID);

            //Assert
            Assert.AreEqual(StatusCodes.Status404NotFound, result);
        }

        [Test]
        public void ValidateBatchID_Returns410Status_WhenBatchIsExpired()
        {
            //Arrange
            var _batchID = Guid.Parse("2d708bc6-8494-4ea4-b632-2da049ef7e9a");

            //Act
            var result = _batchService.ValidateBatchID(_batchID);

            //Assert
            Assert.AreEqual(StatusCodes.Status410Gone, result);
        }
        #endregion

        #region CreateBatch Method Tests

        [Test]
        public void CreateBatch_ReturnGuid_WhenValidBatchRequestIsPassed()
        {
            //Arrange
            var _batchRequest = new BatchRequest();
            _batchRequest.BusinessUnit = "Finance";
            _batchRequest.Acl1 = new BatchRequest.Acl()
            {
                ReadUsers = new string[] { "User1", "User2" },
                ReadGroups = new string[] { "Group2", "Group3" }
            };
            _batchRequest.ExpiryDate = DateTime.Now;
            //Act
            _createBatchID = _batchService.CreateBatch(_batchRequest);

            //Assert
            Assert.IsNotNull(_createBatchID);
        }

        #endregion

        #region Teardown Method
        [TearDown]
        public void BatchServiceTearDown()
        {
            var _batchDetails = _context.Batch.Where(batch => batch.BatchId == _createBatchID.ToString()).ToList().FirstOrDefault();
            if (_batchDetails != null)
            {
                _context.Batch.Remove(_batchDetails);
                _context.SaveChanges();
            }
        }
        #endregion

        #region Private Methods
        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            return config;
        }
        #endregion

    }
}
