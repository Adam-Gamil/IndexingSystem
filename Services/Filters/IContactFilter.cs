using IndexingSystem.Entities;

namespace IndexingSystem.Services.Filters
{
    public interface IContactFilter
    {
        bool Apply(Contact contact);
    }
}