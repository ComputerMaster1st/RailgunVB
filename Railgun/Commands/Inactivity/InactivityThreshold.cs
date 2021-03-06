﻿using Finite.Commands;
using Railgun.Core;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("threshold")]
        public partial class Threshold : SystemBase 
        {
            [Command]
            public Task ExecuteAsync()
                => throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
        }
    }
}
