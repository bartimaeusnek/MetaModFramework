package com.github.bartimaeusnek.metamodjavaclientcore;

import com.github.bartimaeusnek.metamodjavaclientcore.dto.ApiReference;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.SneakyThrows;
import lombok.var;
import org.eclipse.jetty.client.HttpClient;
import org.eclipse.jetty.client.api.ContentResponse;
import org.eclipse.jetty.http.HttpMethod;
import org.eclipse.jetty.util.ssl.SslContextFactory;

import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

@Getter(AccessLevel.PROTECTED)
public abstract class BaseClient {

    private final HttpClient httpClient;
    private final String urlBase;
    private final Gson gson = new GsonBuilder().setFieldNamingStrategy(f -> f.getName().toLowerCase()).create();

    protected BaseClient(String baseUrl) {
        HttpClient client = null;
        String url = null;
        try {
            var sslContextFactory = new SslContextFactory.Client();
            sslContextFactory.setTrustAll(true);
            client = new HttpClient(sslContextFactory);
            client.start();
            baseUrl = baseUrl.trim();
            if (baseUrl.endsWith("/"))
                baseUrl = baseUrl.substring(baseUrl.length() - 1);

            url = "" + baseUrl + "/" + this.getApiVersion(baseUrl, client) + "/";
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            httpClient = client;
            urlBase = url;
        }
    }

    private String getApiVersion(String baseUrl, HttpClient httpClient) throws ExecutionException, InterruptedException, TimeoutException {
        var ret = "v";
        ContentResponse response = httpClient
                .newRequest(baseUrl+"/ApiReference")
                .method(HttpMethod.GET)
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .send();

        var content = response.getContentAsString();
        ret += gson.fromJson(content, ApiReference.class).getVersion();
        return ret;
    }

    @SneakyThrows
    public void stop() {
        httpClient.stop();
    }
}

