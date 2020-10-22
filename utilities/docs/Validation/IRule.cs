namespace docs.Validation
{
    public interface IRule
    {
        void Apply(string path, ValidationResult result);
    }
}
