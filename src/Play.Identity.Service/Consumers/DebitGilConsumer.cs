using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Play.Identity.Contracts;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exceptions;

namespace Play.Identity.Service.Consumers;

public class DebitGilConsumer : IConsumer<DebitGil>
{
    private readonly UserManager<ApplicationUser> userManager;

    public DebitGilConsumer(UserManager<ApplicationUser> userManager)
    {
        this.userManager = userManager;
    }
    public async Task Consume(ConsumeContext<DebitGil> context)
    {
        var message = context.Message;

        var user = await userManager.FindByIdAsync(message.UserId.ToString()) ?? throw new UnknownUserException(message.UserId);

        if( user == null)
        {
            throw new UnknownUserException(message.UserId);
        }
        
        if(user.MessageIds.Contains(context.MessageId.Value)) 
        {
            await context.Publish(new GilDebited(message.CorrelactionId));
            return;
        }

        user.Gil -= message.Gil;

        if(user.Gil < 0) 
        {
            throw new InsufficientFundExceptions(message.UserId, message.Gil);
        }

        user.MessageIds.Add(context.MessageId.Value);
        
        await userManager.UpdateAsync(user);

        var gilDebitedTask = context.Publish(new GilDebited(message.CorrelactionId));
        var userUpdatedTask =  context.Publish(new UserUpdated(user.Id, user.Email, user.Gil));

        await Task.WhenAll(gilDebitedTask, userUpdatedTask);
    }
}

