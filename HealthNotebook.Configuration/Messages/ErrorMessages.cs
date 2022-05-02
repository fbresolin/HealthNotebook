using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthNotebook.Configuration.Messages
{
  public static class ErrorMessages
  {
    public static class Generic
    {
      public static string ObjectNotFound = "Object not found";
      public static string TypeBadRequest = "Bad Request";
      public static string InvalidPayload = "Invalid Payload";
      public static string UnableToProcess = "Unable to process request";
      public static string SomethingWentWrong = "Something went wrong, please try again later";

    }
    public static class Profile
    {
      public static string UserNotFound = "User not found";
    }
    public static class Users
    {
      public static string UserNotFound = "User not found";
    }
  }
}