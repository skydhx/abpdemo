﻿namespace Application.Config;

public class JwtConfig
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string key { get; set; }
}