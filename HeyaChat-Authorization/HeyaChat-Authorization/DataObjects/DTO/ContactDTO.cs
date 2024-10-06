﻿using HeyaChat_Authorization.DataObjects.DTO.SubClasses;

namespace HeyaChat_Authorization.DataObjects.DTO
{
    public class ContactDTO
    {
        public string Contact { get; set; } = "";

        public ResponseDetails Details { get; set; } = null!;
    }
}