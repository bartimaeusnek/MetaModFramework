package com.github.bartimaeusnek.metamodjavaclientcore;

import lombok.AccessLevel;
import lombok.Getter;
import lombok.Setter;

public abstract class AuthorizedClient extends BaseClient {
    public AuthorizedClient(String baseUrl, String token) {
        super(baseUrl);
        this.setToken(token);
    }

    @Getter(AccessLevel.PROTECTED)
    @Setter(AccessLevel.PROTECTED)
    private String Token;
}