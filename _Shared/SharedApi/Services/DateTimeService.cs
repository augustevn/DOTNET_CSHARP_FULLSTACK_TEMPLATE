using SharedApi.Interfaces;

namespace SharedApi.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime Now => DateTime.Now;
}