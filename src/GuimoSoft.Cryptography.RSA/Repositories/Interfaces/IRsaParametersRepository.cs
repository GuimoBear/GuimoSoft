using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace GuimoSoft.Cryptography.RSA.Repositories.Interfaces
{
    public interface IRsaParametersRepository
    {
        ConcurrentDictionary<Guid, RSAParameters> ObterTodos();

        bool TentarObterPorId(Guid id, out RSAParameters parameters);
    }
}
