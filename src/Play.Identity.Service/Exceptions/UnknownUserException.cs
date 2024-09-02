using System;

namespace Play.Identity.Service.Exceptions;

[Serializable]
internal class UnknownUserException : Exception
{
    private Guid userId;

    public UnknownUserException()
    {
    }

    public UnknownUserException(Guid userId)
    {
        this.userId = userId;
    }

    public UnknownUserException(string message) : base(message)
    {
    }

    public UnknownUserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}