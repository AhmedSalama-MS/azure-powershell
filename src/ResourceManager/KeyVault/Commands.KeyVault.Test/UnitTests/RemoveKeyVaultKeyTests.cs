﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Commands.KeyVault.Cmdlets;
using Microsoft.Azure.Commands.KeyVault.Models;
using Microsoft.WindowsAzure.Commands.ScenarioTest;
using Moq;
using System;
using System.Management.Automation;
using Xunit;

namespace Microsoft.Azure.Commands.KeyVault.Test.UnitTests
{
    public class RemoveKeyVaultKeyTests : KeyVaultUnitTestBase
    {
        private RemoveAzureKeyVaultKey cmdlet;
        private KeyAttributes keyAttributes;
        private Microsoft.KeyVault.WebKey.JsonWebKey webKey;
        private KeyBundle keyBundle;

        public RemoveKeyVaultKeyTests()
        {
            base.SetupTest();

            cmdlet = new RemoveAzureKeyVaultKey()
            {
                CommandRuntime = commandRuntimeMock.Object,
                DataServiceClient = keyVaultClientMock.Object,
                VaultName = VaultName
            };

            keyAttributes = new KeyAttributes(true, DateTime.Now, DateTime.Now, "HSM", new string[]{"All"});
            webKey = new Microsoft.KeyVault.WebKey.JsonWebKey();
            keyBundle = new KeyBundle() { Attributes = keyAttributes, Key = webKey, KeyName = KeyName, VaultName = VaultName };
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void CanRemvoeKeyWithPassThruTest()
        {
            KeyBundle expected = keyBundle;
            keyVaultClientMock.Setup(kv => kv.DeleteKey(VaultName, KeyName)).Returns(expected).Verifiable();

            // Mock the should process to return true
            commandRuntimeMock.Setup(cr => cr.ShouldProcess(KeyName, It.IsAny<string>())).Returns(true);
            cmdlet.Name = KeyName;
            cmdlet.Force = true;
            cmdlet.PassThru = true;
            cmdlet.ExecuteCmdlet();

            // Assert
            keyVaultClientMock.VerifyAll();
            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Once());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void CanRemoveKeyWithNoPassThruTest()
        {
            KeyBundle expected = keyBundle;
            keyVaultClientMock.Setup(kv => kv.DeleteKey(VaultName, KeyName)).Returns(expected).Verifiable();

            // Mock the should process to return true
            commandRuntimeMock.Setup(cr => cr.ShouldProcess(KeyName, It.IsAny<string>())).Returns(true);
            cmdlet.Name = KeyName;
            cmdlet.Force = true;
            cmdlet.ExecuteCmdlet();

            keyVaultClientMock.VerifyAll();

            // Without PassThru never call WriteObject
            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Never());
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void CannotRemoveKeyWithoutShouldProcessOrForceConfirmationTest()
        {
            KeyBundle expected = null;

            cmdlet.Name = KeyName;
            cmdlet.PassThru = true;
            cmdlet.ExecuteCmdlet();

            // Write object should be called with null input
            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Once());

            // Should process but without force
            commandRuntimeMock.Setup(cr => cr.ShouldProcess(KeyName, It.IsAny<string>())).Returns(false);
            cmdlet.ExecuteCmdlet();

            // Write object should be called with null input
            commandRuntimeMock.Verify(f => f.WriteObject(expected), Times.Exactly(2));
        }

        [Fact]
        [Trait(Category.AcceptanceType, Category.CheckIn)]
        public void ErrorRemvoeKeyWithPassThruTest()
        {
            keyVaultClientMock.Setup(kv => kv.DeleteKey(VaultName, KeyName)).Throws(new Exception()).Verifiable();

            // Mock the should process to return true
            commandRuntimeMock.Setup(cr => cr.ShouldProcess(KeyName, It.IsAny<string>())).Returns(true);
            cmdlet.Name = KeyName;
            cmdlet.Force = true;
            cmdlet.PassThru = true;
            cmdlet.ExecuteCmdlet();

            // Assert
            keyVaultClientMock.VerifyAll();
            commandRuntimeMock.Verify(f => f.WriteError(It.IsAny<ErrorRecord>()), Times.Once());
        }
    }
}
