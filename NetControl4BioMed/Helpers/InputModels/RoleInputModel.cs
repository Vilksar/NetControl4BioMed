using System.Collections.Generic;

namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a role.
    /// </summary>
    public class RoleInputModel
    {
        /// <summary>
        /// Represents the ID of the role.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the name of the role.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Represents the user roles of the role.
        /// </summary>
        public IEnumerable<UserRoleInputModel> UserRoles { get; set; }
    }
}
