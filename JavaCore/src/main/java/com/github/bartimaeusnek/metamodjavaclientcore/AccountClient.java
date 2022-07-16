package com.github.bartimaeusnek.metamodjavaclientcore;

import lombok.var;
import org.eclipse.jetty.http.HttpMethod;
import org.eclipse.jetty.http.HttpStatus;

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

        return response.getStatus() == HttpStatus.ACCEPTED_202;
    }

    /**
     * @return null if BadRequest/400, JWT Token if OK/200,
     * @throws IllegalStateException otherwise
     */
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

        var status = response.getStatus();

        if (status == HttpStatus.BAD_REQUEST_400)
            return null;

        if (status == HttpStatus.UNAUTHORIZED_401)
            throw new IllegalStateException("Login Data Incorrect");

        if (status != HttpStatus.OK_200)
            throw new IllegalStateException(response.getReason());

        return response.getContentAsString();
    }
}