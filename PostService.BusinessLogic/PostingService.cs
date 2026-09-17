using PostService.CommonTypes;
using PostService.DataAccess;
using PostService.Models;

namespace PostService.BusinessLogic;

public class PostingService : IPostingService
{
    private readonly IPostingRepository _repository;

    public PostingService(IPostingRepository repository)
    {
        _repository = repository;
    }

    public Posting Create(Posting newPosting)
    {
        newPosting.CreatedAt = DateTime.UtcNow;

        float baseRate = newPosting.DeliveryType switch
        {
            DeliveryType.Department => 40f,
            DeliveryType.Courier => 80f,
            DeliveryType.ExpressCourier => 120f,
            _ => 40f
        };

        float perKgRate = newPosting.DeliveryType switch
        {
            DeliveryType.Department => 10f,
            DeliveryType.Courier => 15f,
            DeliveryType.ExpressCourier => 24f,
            _ => 10f
        };

        newPosting.Price = baseRate + (newPosting.Weight * perKgRate);

        var id = _repository.Create(newPosting);
        newPosting.Id = id;
        return newPosting;
    }

    public List<Posting> GetAll() => _repository.GetList();

    public Posting? Find(int postingId) => _repository.GetById(postingId);

    public Posting? Update(Posting posting)
    {
        var existing = _repository.GetById(posting.Id);
        if (existing is null)
        {
            return null;
        }

        posting.CreatedAt = existing.CreatedAt;
        var rowsAffected = _repository.Update(posting);
        return rowsAffected > 0 ? posting : null;
    }

    public int Delete(int postingId) => _repository.Delete(postingId);
}