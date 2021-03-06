﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ardoq;
using Ardoq.Models;
using Ardoq.Service;
using Ardoq.Service.Interface;
using ArdoqTest.Helper;
using NUnit.Framework;
using Refit;

namespace ArdoqTest.Service
{
    [TestFixture]
    public class ComponentServiceTests
    {
        private IComponentService service;
        private IArdoqClient client;

        [OneTimeSetUp]
        public void Setup()
        {
            client = TestUtils.GetClient();
            service = client.ComponentService;
        }

        private async Task<Workspace> CreateWorkspace()
        {
            Workspace workspace =
                await
                    client.WorkspaceService.CreateWorkspace(
                        new Workspace("My Component Test Workspace",
                        TestUtils.GetTestPropery("modelId"), "Hello world!"));
            return workspace;
        }

        private Component CreateComponentTemplate(Workspace workspace)
        {
            return new Component("My Component Test", workspace.Id, "myDescription");
        }

        private async Task DeleteWorkspace(Workspace workspace)
        {
            await client.WorkspaceService.DeleteWorkspace(workspace.Id);
        }

        [Test]
        public async Task CreateComponentTest()
        {
            Workspace workspace = await CreateWorkspace();
            Component componentTemplate = CreateComponentTemplate(workspace);
            Component component = await service.CreateComponent(componentTemplate);
            Assert.NotNull(component.Id);
            await service.DeleteComponent(component.Id);
            await DeleteWorkspace(workspace);
        }

        [Test]
        public async Task DeleteComponentTest()
        {
            Workspace workspace = await CreateWorkspace();
            Component componentTemplate = CreateComponentTemplate(workspace);
            Component result = await service.CreateComponent(componentTemplate);
            Assert.NotNull(result.Id);
            await service.DeleteComponent(result.Id);
            try
            {
                await service.GetComponentById(result.Id);
                await DeleteWorkspace(workspace);
                Assert.Fail("Expected the Component to be deleted.");
            }
            catch (Refit.ApiException e)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, e.StatusCode);
            }
            await DeleteWorkspace(workspace);
        }

        [Test]
        public async Task GetComponentTest()
        {
            Workspace workspace = await CreateWorkspace();
            Component componentTemplate = CreateComponentTemplate(workspace);
            Component component = await service.CreateComponent(componentTemplate);
            List<Component> allComponents = await service.GetAllComponents();
            string id = allComponents[0].Id;
            Component expectedComponent = await service.GetComponentById(id);
            Assert.True(id == expectedComponent.Id);
            await service.DeleteComponent(component.Id);
            await DeleteWorkspace(workspace);
        }

        [Test]
        public async Task UpdateComponentTest()
        {
            Workspace workspace = await CreateWorkspace();
            Component componentTemplate = CreateComponentTemplate(workspace);
            Component component = await service.CreateComponent(componentTemplate);
            component.Description = "Updated description";
            Component updatedComponent = await service.UpdateComponent(component.Id, component);
            Assert.True("Updated description" == updatedComponent.Description);
            Assert.True(component.VersionCounter == updatedComponent.VersionCounter - 1);
            await service.DeleteComponent(component.Id);
            await DeleteWorkspace(workspace);
        }
    }
}