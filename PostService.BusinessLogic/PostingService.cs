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

        float baseRate;
        float perKgRate;

        if (newPosting.DeliveryType == DeliveryType.Department)
        {
            baseRate = 40f;
            perKgRate = 10f;
        }
        else if (newPosting.DeliveryType == DeliveryType.Courier)
        {
            baseRate = 80f;
            perKgRate = 15f;
        }
        else if (newPosting.DeliveryType == DeliveryType.ExpressCourier)
        {
            baseRate = 120f;
            perKgRate = 24f;
        }
        else
        {
            baseRate = 40f;
            perKgRate = 10f;
        }

        newPosting.Price = baseRate + (newPosting.Weight * perKgRate);

        var id = _repository.Create(newPosting);
        newPosting.Id = id;
        return newPosting;
    }

    public List<Posting> GetAll()
    {
        return _repository.GetList();
    }

    public Posting? Find(int postingId)
    {
        return _repository.GetById(postingId);
    }

    public Posting? Update(Posting posting)
    {
        var existing = _repository.GetById(posting.Id);
        if (existing is null)
        {
            return null;
        }

        posting.CreatedAt = existing.CreatedAt;
        var rowsAffected = _repository.Update(posting);
        if (rowsAffected > 0)
        {
            return posting;
        }

        return null;
    }

    public int Delete(int postingId)
    {
        return _repository.Delete(postingId);
    }
}