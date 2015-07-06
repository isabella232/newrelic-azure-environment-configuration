using EnvVars.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnvVars.Tests
{
    [TestClass]
    public class RoleModelTests
    {
        [TestMethod]
        public void Set_RoleName_Returns_Value()
        {
            var role = new RoleModel {RoleName = "name", DeploymentSlot = "staging", Location = "West US"};
            Assert.AreEqual("name", role.RoleName);
        }

        [TestMethod]
        public void Set_DeploymentSlot_Returns_Value()
        {
            var role = new RoleModel {RoleName = "name", DeploymentSlot = "staging", Location = "West US"};
            Assert.AreEqual("staging", role.DeploymentSlot);
        }

        [TestMethod]
        public void Set_Location_Returns_Value()
        {
            var role = new RoleModel {RoleName = "name", DeploymentSlot = "staging", Location = "West US"};
            Assert.AreEqual("West US", role.Location);
        }

        [TestMethod]
        public void Format_Name_Returns_Correct_Fragments()
        {
            var role = new RoleModel {RoleName = "name", DeploymentSlot = "staging", Location = "West US"};
            var formattedName = role.FormatName("rp");

            Assert.AreEqual("rp.staging.westus", formattedName);
        }

    }
}
