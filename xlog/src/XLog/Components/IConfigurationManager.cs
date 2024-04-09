using System;

namespace XLog.Components;

public interface IConfigurationManager
{
    void GraylogLogin(string name, string server, string login, string pass);
    (string server, string login, string pass)? GraylogGetCredentials(string name);
    void GraylogLogout(string name);
}
