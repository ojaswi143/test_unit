using ESG_Survey_Automation.Domain;
using ESG_Survey_Automation.Controller;
using ESG_Survey_Automation.Infrastructure.EntityFramework;
using ESG_Survey_Automation.Infrastructure.EntityFramework.Models;
using ESG_Survey_Automation.Domain;
using ESG_Survey_Automation.Infrastructure.FileStorage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ESG_Survey_Automation.Controller;

namespace ESG_Survey_AutomationTest
{
    public class SurveyControllerTest
    {
        [Fact]
        public async Task AskQuestion_WithValidQuestion_ShouldReturnOkResult()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<AiController>>();

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"answer\": \"test answer\"}")
            };

            var client = new HttpClient(new Mock<HttpMessageHandler>().Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var controller = new SurveyController(mockFactory.Object, mockConfiguration.Object, mockLogger.Object);

            // Act
            var result = await controller.AskQuestion("test question");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var answer = Assert.IsType<AnswerModel>(okResult.Value);
            Assert.Equal("test answer", answer.Answer);
        }

        [Fact]
        public async Task AskQuestion_WithEmptyQuestion_ShouldReturnBadRequest()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<AiController>>();

            var controller = new SurveyController(mockFactory.Object, mockConfiguration.Object, mockLogger.Object);

            // Act
            var result = await controller.AskQuestion("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid question", badRequestResult.Value);
        }

        [Fact]
        public async Task AskQuestion_WithFailedResponse_ShouldReturnStatusCode()
        {
            // Arrange
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<AiController>>();

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            var client = new HttpClient(new Mock<HttpMessageHandler>().Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };

            mockFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            var controller = new SurveyController(mockFactory.Object, mockConfiguration.Object, mockLogger.Object);

            // Act
            var result = await controller.AskQuestion("test question");

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)HttpStatusCode.BadRequest, statusCodeResult.StatusCode);
        }

        [Fact]
        public async Task UploadSurveyQuestionAir_WithValidFile_ShouldReturnOkResult()
        {
            // Arrange
            var mockFileStorage = new Mock<IFileStorageService>();
            var mockLogger = new Mock<ILogger<FileController>>();

            var controller = new SurveyController(mockFileStorage.Object, mockLogger.Object);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(100); // Set file length

            // Act
            var result = await controller.UploadSurveyQuestionAir(file.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("File uploaded successfully.", okResult.Value);
        }

        [Fact]
        public async Task UploadSurveyQuestionAir_WithNoFile_ShouldReturnBadRequest()
        {
            // Arrange
            var mockFileStorage = new Mock<IFileStorageService>();
            var mockLogger = new Mock<ILogger<FileController>>();

            var controller = new SurveyController(mockFileStorage.Object, mockLogger.Object);

            // Act
            var result = await controller.UploadSurveyQuestionAir(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded.", badRequestResult.Value);
        }

        [Fact]
        public async Task UploadSurveyQuestionAir_WithFailedUpload_ShouldReturnInternalServerError()
        {
            // Arrange
            var mockFileStorage = new Mock<IFileStorageService>();
            var mockLogger = new Mock<ILogger<FileController>>();

            var controller = new SurveyController(mockFileStorage.Object, mockLogger.Object);
            var file = new Mock<IFormFile>();
            file.Setup(f => f.Length).Returns(100); // Set file length

            mockFileStorage.Setup(f => f.UploadFileToCloud(It.IsAny<IFormFile>())).ThrowsAsync(new Exception("Upload failed"));

            // Act
            var result = await controller.UploadSurveyQuestionAir(file.Object);

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        }
    }
}