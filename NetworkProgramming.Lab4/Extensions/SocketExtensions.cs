﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace NetworkProgramming.Lab4.Extensions
{
   public static class SocketExtensions
   {
      public static bool IsDisposed(this Socket socket)
      {
         BindingFlags bfIsDisposed = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetProperty;
         // Retrieve a FieldInfo instance corresponding to the field
         PropertyInfo field = socket.GetType().GetProperty("CleanedUp", bfIsDisposed);
         // Retrieve the value of the field, and cast as necessary
         return (bool)field.GetValue(socket, null);
      }
   }
}
