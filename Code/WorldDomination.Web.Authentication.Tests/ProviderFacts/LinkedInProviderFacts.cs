﻿using System;
using System.Collections.Specialized;
using System.Net;
using Moq;
using RestSharp;
using WorldDomination.Web.Authentication.ExtraProviders;
using WorldDomination.Web.Authentication.ExtraProviders.LinkedIn;
using Xunit;
using AccessTokenResult = WorldDomination.Web.Authentication.Providers.Google.AccessTokenResult;
using UserInfoResult = WorldDomination.Web.Authentication.Providers.Google.UserInfoResult;

namespace WorldDomination.Web.Authentication.Tests.ProviderFacts
{
    // ReSharper disable InconsistentNaming

    public class LinkedInProviderFacts
    {
        public class AuthenticateClientFacts
        {
            [Fact]
            public void GivenLinkedInReturnedAnError_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"});
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {
                        "error",
                        "I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers."
                    },
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState
                };
                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to retrieve an authorization code from LinkedIn. The error provided is: I dont' always use bayonets. But when I do, I transport them on Aircraft Carriers.",
                    result.Message);
            }

            [Fact]
            public void GivenNoCodeAndNoErrorWasReturned_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var linkedinProvider = new LinkedInProvider(new ProviderParams { Key = "aa", Secret = "bb" });
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"aaa", "bbb"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("No code parameter provided in the response query string from LinkedIn.", result.Message);
            }

            [Fact]
            public void GivenANullCallbackUriWhileTryingToRetrieveAnAccessToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Bad Request");
                mockRestResponse.Setup(x => x.Content).Returns("{\n  \"error\" : \"invalid_request\"\n}");
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"})
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://2p1s.com"),
                    State = existingState
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from LinkedIn OR the the response was not an HTTP Status 200 OK. Response Status: BadRequest. Response Description: Bad Request",
                    result.Message);
            }

            [Fact]
            public void GivenAnErrorOccuredWhileTryingToRetrieveAnAccessToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                const string errorMessage = "If God says he was not created by a creator, does that mean: god is an aetheist?";
                mockRestClient.Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                              .Throws(new InvalidOperationException(errorMessage));
                var linkedinProvider = new LinkedInProvider(new ProviderParams { Key = "aa", Secret = "bb" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("Failed to obtain an Access Token from LinkedIn OR the the response was not an HTTP Status 200 OK. Response Status: -- null response --. Response Description: ", result.Message);
                Assert.NotNull(result.InnerException);
                Assert.Equal(errorMessage, result.InnerException.Message);
            }

            [Fact]
            public void GivenAnInvalidRequestToken_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponse.Setup(x => x.StatusDescription).Returns("Unauthorized");
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var linkedinProvider = new LinkedInProvider(new ProviderParams { Key = "aa", Secret = "bb" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    CallBackUri = new Uri("http://2p1s.com"),
                    State = existingState
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain an Access Token from LinkedIn OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized",
                    result.Message);
            }

            [Fact]
            public void GivenAnRequestTokenWithMissingParameters_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestClient = new Mock<IRestClient>();
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult());
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);
                var linkedinProvider = new LinkedInProvider(new ProviderParams { Key = "aa", Secret = "bb" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Retrieved a LinkedIn Access Token but it doesn't contain one or more of either: access_token or token_type",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingUserInfoThrowsAnException_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    AccessToken = "aaa",
                    TokenType = "overly attached girlfriend"
                });

                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.Unauthorized);
                mockRestResponseUserInfo.Setup(x => x.StatusDescription).Returns("Unauthorized");

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);

                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);

                var linkedinProvider = new LinkedInProvider(new ProviderParams { Key = "aa", Secret = "bb" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Failed to obtain User Info from LinkedIn OR the the response was not an HTTP Status 200 OK. Response Status: Unauthorized. Response Description: Unauthorized",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingUserInfoWorksButIsMissingSomeRequiredData_AuthenticateClient_ThrowsAnException()
            {
                // Arrange.
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    AccessToken = "aaa",
                    TokenType = "overly attached girlfriend"
                });

                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseUserInfo.Setup(x => x.Data).Returns(new UserInfoResult()); // Missing required info.

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);

                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);

                var linkedinProvider = new LinkedInProvider(new ProviderParams { Key = "aa", Secret = "bb" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";
                var queryStringParameters = new NameValueCollection
                {
                    {"code", "aaa"},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = Assert.Throws<AuthenticationException>(
                    () => linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters));

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "Retrieve some user info from the LinkedIn Api, but we're missing one or more of either: Id, Login, and Name.",
                    result.Message);
            }

            [Fact]
            public void GivenExecutingRetrieveSomeUserInfo_AuthenticateClient_ReturnsAnAuthenticatedClient()
            {
                // Arrange.
                const string accessToken = "aaa";
                var mockRestResponse = new Mock<IRestResponse<AccessTokenResult>>();
                mockRestResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponse.Setup(x => x.Data).Returns(new AccessTokenResult
                {
                    AccessToken = accessToken,
                    TokenType = "overly attached girlfriend"
                });

                var userInfoResult = new UserInfoResult
                {
                    Email = "aaa",
                    Id = "1",
                    Name = "eee"
                };
                var mockRestResponseUserInfo = new Mock<IRestResponse<UserInfoResult>>();
                mockRestResponseUserInfo.Setup(x => x.StatusCode).Returns(HttpStatusCode.OK);
                mockRestResponseUserInfo.Setup(x => x.Data).Returns(userInfoResult);

                var mockRestClient = new Mock<IRestClient>();
                mockRestClient
                    .Setup(x => x.Execute<AccessTokenResult>(It.IsAny<IRestRequest>()))
                    .Returns(mockRestResponse.Object);

                mockRestClient.
                    Setup(x => x.Execute<UserInfoResult>(It.IsAny<IRestRequest>()))
                              .Returns(mockRestResponseUserInfo.Object);

                var linkedinProvider = new LinkedInProvider(new ProviderParams { Key = "aa", Secret = "bb" })
                {
                    RestClientFactory = new RestClientFactory(mockRestClient.Object)
                };
                const string existingState = "Oops! - Tasselhoff Burrfoot";

                var queryStringParameters = new NameValueCollection
                {
                    {"code", accessToken},
                    {"state", existingState}
                };
                var linkedInAuthenticationServiceSettings = new LinkedInAuthenticationServiceSettings
                {
                    State = existingState,
                    CallBackUri = new Uri("http://2p1s.com")
                };

                // Act.
                var result = linkedinProvider.AuthenticateClient(linkedInAuthenticationServiceSettings, queryStringParameters);

                // Assert.
                Assert.NotNull(result);
                Assert.Equal("linkedin", result.ProviderName);
                Assert.Equal(accessToken, result.AccessToken);
                Assert.Equal(new DateTime(),result.AccessTokenExpiresOn);
                Assert.NotNull(result.UserInformation);
                Assert.Equal(GenderType.Unknown, result.UserInformation.Gender);
                Assert.Equal(userInfoResult.Id, result.UserInformation.Id);
                Assert.Equal(userInfoResult.Name, result.UserInformation.Name);
            }
        }

        public class RedirectToAuthenticateFacts
        {
            [Fact]
            public void GivenSomeState_RedirectToAuthenticate_ReturnsAUri()
            {
                // Arrange.
                var linkedinProvider = new LinkedInProvider(new ProviderParams {Key = "aa", Secret = "bb"});

                // Act.
                var result =
                    linkedinProvider.RedirectToAuthenticate(new LinkedInAuthenticationServiceSettings
                    {
                        CallBackUri =
                            new Uri("http://wwww.pewpew.com/"),
                        State = "bleh"
                    });

                // Assert.
                Assert.NotNull(result);
                Assert.Equal(
                    "https://linkedin.com/login/oauth/authorize?client_id=aa&redirect_uri=http://wwww.pewpew.com/&response_type=code&state=bleh",
                    result.AbsoluteUri);
            }
        }
    }

    // ReSharper restore InconsistentNaming
}