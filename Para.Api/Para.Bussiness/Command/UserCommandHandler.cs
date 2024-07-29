using AutoMapper;
using MediatR;
using Newtonsoft.Json;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Bussiness.RabbitMQ;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Command;

public class UserCommandHandler :
    IRequestHandler<CreateUserCommand, ApiResponse<UserResponse>>,
    IRequestHandler<UpdateUserCommand, ApiResponse>,
    IRequestHandler<DeleteUserCommand, ApiResponse>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly RabbitMQClient _rabbitMQClient;

    public UserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, RabbitMQClient rabbitMQClient)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        _rabbitMQClient = rabbitMQClient;
    }

    public async Task<ApiResponse<UserResponse>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var mapped = mapper.Map<UserRequest, User>(request.Request);
        mapped.Status = 1;
        await unitOfWork.UserRepository.Insert(mapped);
        await unitOfWork.Complete();

        var response = mapper.Map<UserResponse>(mapped);

        // Mesajý RabbitMQ kuyruðuna göndermek
        var emailMessage = new
        {
            To = request.Request.Email, // E-posta adresini request'ten al
            Subject = "Welcome to our service!",
            Body = $"Hello {request.Request.FirstName}, your account has been created."
        };

        var jsonMessage = JsonConvert.SerializeObject(emailMessage);
        // RabbitMQClient servisi kullanarak mesaj gönderimi
        await _rabbitMQClient.SendMessageAsync(jsonMessage, "emailQueue");

        return new ApiResponse<UserResponse>(response);
    }

    public async Task<ApiResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var mapped = mapper.Map<UserRequest, User>(request.Request);
        mapped.Id = request.UserId;
        unitOfWork.UserRepository.Update(mapped);
        await unitOfWork.Complete();
        return new ApiResponse();
    }

    public async Task<ApiResponse> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.UserRepository.Delete(request.UserId);
        await unitOfWork.Complete();
        return new ApiResponse();
    }
}