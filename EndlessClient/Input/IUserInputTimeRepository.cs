// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EndlessClient.Input
{
    public interface IUserInputTimeRepository
    {
        DateTime LastInputTime { get; set; }
    }

    public class UserInputTimeRepository : IUserInputTimeRepository
    {
        public DateTime LastInputTime { get; set; }

        public UserInputTimeRepository()
        {
            LastInputTime = DateTime.Now;
        }
    }
}
