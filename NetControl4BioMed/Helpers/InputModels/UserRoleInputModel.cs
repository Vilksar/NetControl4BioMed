namespace NetControl4BioMed.Helpers.InputModels
{
    /// <summary>
    /// Represents the input model for a user role.
    /// </summary>
    public class UserRoleInputModel
    {
        /// <summary>
        /// Represents the user of the user role.
        /// </summary>
        public UserInputModel User { get; set; }

        /// <summary>
        /// Represents the role of the user role.
        /// </summary>
        public RoleInputModel RoleId { get; set; }
    }
}
