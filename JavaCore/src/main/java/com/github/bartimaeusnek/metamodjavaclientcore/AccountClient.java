package com.github.bartimaeusnek.metamodjavaclientcore;

import lombok.var;
import org.eclipse.jetty.http.HttpMethod;

import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

public class AccountClient extends BaseClient
    {
        public AccountClient(String baseUrl) {
            super(baseUrl);
        }

        public boolean registerAsync(String name, String email, String password) throws ExecutionException, InterruptedException, TimeoutException {

            var response = getHttpClient()
                    .newRequest(getUrlBase()+"Register")
                    .method(HttpMethod.POST)
                    .timeout(1, TimeUnit.MINUTES)
                    .idleTimeout(1, TimeUnit.MINUTES)
                    .param("name",name)
                    .param("email",email)
                    .param("password",password)
                    .send();

            return response.getStatus() == 202;
        }

        public String loginAsync(String userName, String password, String audience) throws ExecutionException, InterruptedException, TimeoutException {

            var response = getHttpClient()
                    .newRequest(getUrlBase()+"Login")
                    .method(HttpMethod.POST)
                    .timeout(1, TimeUnit.MINUTES)
                    .idleTimeout(1, TimeUnit.MINUTES)
                    .param("userName",userName)
                    .param("audience",audience)
                    .param("password",password)
                    .send();

            return response.getContentAsString();
        }
    }