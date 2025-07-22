using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Xunit;
using ECommerce.BusinessEvents.Services;

namespace ECommerce.BusinessEvents.Tests.Services
{
    public class JsonPatchUtilityTests
    {
        [Fact]
        public void GeneratePatch_NoChanges_ReturnsEmptyPatch()
        {
            var json = "{\"name\":\"John\",\"age\":30}";
            var patch = JsonPatchUtility.GeneratePatch(json, json);
            Assert.Empty(patch.Operations);
        }

        [Fact]
        public void GeneratePatch_PropertyAdded_ReturnsAddOperation()
        {
            var oldJson = "{\"name\":\"John\"}";
            var newJson = "{\"name\":\"John\",\"age\":30}";
            var patch = JsonPatchUtility.GeneratePatch(oldJson, newJson);
            Assert.Contains(patch.Operations, op => op.op == "add" && op.path == "/age");
        }

        [Fact]
        public void GeneratePatch_PropertyRemoved_ReturnsRemoveOperation()
        {
            var oldJson = "{\"name\":\"John\",\"age\":30}";
            var newJson = "{\"name\":\"John\"}";
            var patch = JsonPatchUtility.GeneratePatch(oldJson, newJson);
            Assert.Contains(patch.Operations, op => op.op == "remove" && op.path == "/age");
        }

        [Fact]
        public void GeneratePatch_PropertyChanged_ReturnsReplaceOperation()
        {
            var oldJson = "{\"name\":\"John\"}";
            var newJson = "{\"name\":\"Jane\"}";
            var patch = JsonPatchUtility.GeneratePatch(oldJson, newJson);
            Assert.Contains(patch.Operations, op => op.op == "replace" && op.path == "/name");
        }

        [Fact]
        public void GeneratePatch_ArrayChanged_ReturnsReplaceOperation()
        {
            var oldJson = "{\"items\":[1,2,3]}";
            var newJson = "{\"items\":[1,2,4]}";
            var patch = JsonPatchUtility.GeneratePatch(oldJson, newJson);
            Assert.Contains(patch.Operations, op => op.op == "replace" && op.path == "/items");
        }

        [Fact]
        public void GeneratePatch_TypeChanged_ReturnsReplaceOperation()
        {
            var oldJson = "{\"active\":true}";
            var newJson = "{\"active\":\"yes\"}";
            var patch = JsonPatchUtility.GeneratePatch(oldJson, newJson);
            Assert.Contains(patch.Operations, op => op.op == "replace" && op.path == "/active");
        }
    }
}

