// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Controllers.Repositories
{
    public interface ILoginControllerRepository
    {
        ILoginController LoginController { get; set; }
    }

    public class LoginControllerRepository : ILoginControllerRepository
    {
        public ILoginController LoginController { get; set; }
    }
}
