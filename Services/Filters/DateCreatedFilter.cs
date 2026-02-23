using IndexingSystem.Entities;

namespace IndexingSystem.Services.Filters
{
    public class DateCreatedFilter : IContactFilter
    {
        private readonly DateTime _targetDate;
        private readonly DateComparisonType _comparisonType;

        public DateCreatedFilter(DateTime targetDate, DateComparisonType comparisonType)
        {
            _targetDate = targetDate.Date;
            _comparisonType = comparisonType;
        }

        public bool Apply(Contact contact)
        {
            var contactDate = contact.CreatedAt.Date;

            switch (_comparisonType)
            {
                case DateComparisonType.Before:
                    return contactDate < _targetDate;

                case DateComparisonType.After:
                    return contactDate > _targetDate;

                case DateComparisonType.OnExactDate:
                    return contactDate == _targetDate;

                default:
                    return false;
            }
        }
    }
}