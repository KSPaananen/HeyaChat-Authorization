﻿using HeyaChat_Authorization.DataObjects.DRO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DRO
{
    public class VerifyEmailDRO
    {
        public string Code { get; set; } = "";

        public UserDevice Device {  get; set; } = null!;
    }
}