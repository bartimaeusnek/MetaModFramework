package com.github.bartimaeusnek.metamodjavaclientcore;

import lombok.var;
import org.eclipse.jetty.http.HttpHeader;
import org.eclipse.jetty.http.HttpMethod;

import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

public class TransactionService extends AuthorizedClient {
    public TransactionService(String baseUrl, String token) {
        super(baseUrl, token);
    }

    public boolean getLock() throws ExecutionException, InterruptedException, TimeoutException {
        var response = getHttpClient()
                .newRequest(getUrlBase()+"Transaction")
                .method(HttpMethod.GET)
                .header(HttpHeader.AUTHORIZATION, "Bearer " + this.getToken())
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .send();

        return response.getStatus() == 200;
    }

    public boolean postLock() throws ExecutionException, InterruptedException, TimeoutException {
        var response = getHttpClient()
                .newRequest(getUrlBase()+"Transaction")
                .method(HttpMethod.POST)
                .header(HttpHeader.AUTHORIZATION, "Bearer " + this.getToken())
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .send();

        return response.getStatus() == 200;
    }
}
