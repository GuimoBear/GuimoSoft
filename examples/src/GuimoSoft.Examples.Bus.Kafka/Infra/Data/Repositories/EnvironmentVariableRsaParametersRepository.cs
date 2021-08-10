using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using GuimoSoft.Cryptography.RSA.Packets;
using GuimoSoft.Cryptography.RSA.Repositories.Interfaces;

namespace GuimoSoft.Examples.Bus.Kafka.Infra.Data.Repositories
{
    public class EnvironmentVariableRsaParametersRepository : IRsaParametersRepository
    {
        private readonly ConcurrentDictionary<Guid, RSAParameters> certificates
            = new ConcurrentDictionary<Guid, RSAParameters>();

        private readonly string _certPreffix;
        private readonly string _password;

        public EnvironmentVariableRsaParametersRepository(string certPreffix = "CERT_", string password = default)
        {
            _certPreffix = certPreffix;
            _password = password;
        }

        public ConcurrentDictionary<Guid, RSAParameters> ObterTodos()
        {
            var envs = GetAllEnvironmentVariables();
            foreach (var (key, value) in envs.Where(kvp => kvp.Key.StartsWith(_certPreffix)))
            {
                var content = Convert.FromBase64String(value);
                var package = string.IsNullOrEmpty(_password) ?
                    new RsaParametersPackage(content) :
                    new RsaParametersPackage(new PasswordEncryptedPackage(content).Decrypt(_password));
                if (!certificates.ContainsKey(package.Identifier))
                    certificates.TryAdd(package.Identifier, package.Content);
            }
            return certificates;
        }

        public bool TentarObterPorId(Guid id, out RSAParameters parameters)
        {
            if (!certificates.TryGetValue(id, out parameters))
            {
                var envContent = Environment.GetEnvironmentVariable($"{_certPreffix}{id.ToString("N")}");
                if (string.IsNullOrEmpty(envContent))
                    return false;
                var content = Convert.FromBase64String(envContent);
                var package = string.IsNullOrEmpty(_password) ?
                    new RsaParametersPackage(content) :
                    new RsaParametersPackage(new PasswordEncryptedPackage(content).Decrypt(_password));
                certificates.TryAdd(package.Identifier, package.Content);
                parameters = package.Content;
                return true;
            }
            return false;
        }

        private static IDictionary<string, string> GetAllEnvironmentVariables()
        {
            var ret = new Dictionary<string, string>();
            var dictEnumerator = Environment.GetEnvironmentVariables().GetEnumerator();
            while (dictEnumerator.MoveNext())
            {
                ret.Add(dictEnumerator.Key.ToString(), dictEnumerator.Value?.ToString());
            }
            return ret;
        }
    }
}
