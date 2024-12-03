﻿using MediaServer.SDP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServer.SDP.Interfaces
{

    public interface ISDPValidator
    {
        ValidationResult Validate(SessionDescription session);
    }



}