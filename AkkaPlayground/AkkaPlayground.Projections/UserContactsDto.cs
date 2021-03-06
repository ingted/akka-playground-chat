﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaPlayground.Projections
{
    public class UserContactsDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ContactUserId { get; set; }

        public string ContactLogin { get; set; }

        public string ContactUserName { get; set; }
    }
}
