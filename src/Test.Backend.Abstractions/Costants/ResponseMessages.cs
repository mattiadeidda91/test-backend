namespace Test.Backend.Abstractions.Costants
{
    public static class ResponseMessages
    {
        public const string CreatedSuccessfull = "{0} created correctly.";
        public const string Conflict = "Conflict! {0} with Id '{1}' already exists.";
        public const string GuidEmpty = "{0} Id cannot be empty.";
        public const string MappingNull = "{0} mapping error. {0} entity is null.";
        public const string GenericError = "{0} cannot be {1}.";
        public const string GetByIdNotFound = "{0} with Id '{1}' not found.";
        public const string GetNotFound = "No {0} found.";
    }
}
