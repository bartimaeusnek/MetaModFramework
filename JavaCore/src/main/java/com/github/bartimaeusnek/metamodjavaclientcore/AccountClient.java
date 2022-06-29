package com.github.bartimaeusnek.metamodjavaclientcore;

import lombok.var;
import org.eclipse.jetty.http.HttpMethod;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.Base64;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

public class AccountClient extends BaseClient {
    public AccountClient(String baseUrl) {
        super(baseUrl);
        try {
            digest = MessageDigest.getInstance("SHA-256");
        } catch (NoSuchAlgorithmException e) {
            throw new RuntimeException(e);
        }
    }

    private final MessageDigest digest;

    public boolean registerAsync(String name, String email, String password) throws ExecutionException, InterruptedException, TimeoutException {
        var hashedPassword = Base64.getEncoder().encodeToString(digest.digest(password.getBytes(StandardCharsets.UTF_8)));

        var response = getHttpClient()
                .newRequest(getUrlBase() + "Register")
                .method(HttpMethod.POST)
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .param("name", name)
                .param("email", email)
                .param("password", hashedPassword)
                .send();

        return response.getStatus() == 202;
    }

    public String loginAsync(String userName, String password, String audience) throws ExecutionException, InterruptedException, TimeoutException, IllegalStateException {
        var hashedPassword = Base64.getEncoder().encodeToString(digest.digest(password.getBytes(StandardCharsets.UTF_8)));

        var response = getHttpClient()
                .newRequest(getUrlBase() + "Login")
                .method(HttpMethod.POST)
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .param("userName", userName)
                .param("audience", audience)
                .param("password", hashedPassword)
                .send();

        if (response.getStatus() != 200)
            throw new IllegalStateException("Login Data Incorrect");

        return response.getContentAsString();
    }
}