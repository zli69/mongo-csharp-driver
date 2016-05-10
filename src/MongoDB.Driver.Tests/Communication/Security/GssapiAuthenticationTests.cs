/* Copyright 2010-2015 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Configuration;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Communication.Security
{
    [TestFixture]
    [Explicit]
    [Category("Authentication")]
    [Category("GssapiMechanism")]
    public class GssapiAuthenticationTests
    {
        private static readonly string __collectionName = "test";

        private MongoClientSettings _settings;

        [SetUp]
        public void Setup()
        {
            _settings = MongoClientSettings.FromUrl(new MongoUrl(CoreTestConfiguration.ConnectionString.ToString()));
        }

        [Test]
        public void TestNoCredentials()
        {
            _settings.Credentials = Enumerable.Empty<MongoCredential>();
            var client = new MongoClient(_settings);

            Assert.Throws<MongoCommandException>(() =>
            {
                client
                    .GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName)
                    .GetCollection<BsonDocument>(__collectionName)
                    .Count(new BsonDocument());
            });
        }


        [Test]
        public void TestSuccessfulAuthentication()
        {
            var client = new MongoClient(_settings);

            var result = client
                .GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName)
                .GetCollection<BsonDocument>(__collectionName)
                .FindSync(new BsonDocument())
                .ToList();

            Assert.IsNotNull(result);
        }

        [Test]
        public void TestBadPassword()
        {
            var currentCredentialUsername = _settings.Credentials.Single().Username;
            _settings.Credentials = new[]
            {
                MongoCredential.CreateGssapiCredential(currentCredentialUsername, "wrongPassword")
            };

            var client = new MongoClient(_settings);

            Assert.Throws<TimeoutException>(() =>
            {
                client
                    .GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName)
                    .GetCollection<BsonDocument>(__collectionName)
                    .FindSync(new BsonDocument())
                    .ToList();
            });
        }
    }
}