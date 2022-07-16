package com.github.bartimaeusnek.metamodjavaclientcore;

import com.github.bartimaeusnek.metamodjavaclientcore.dto.ClientItem;
import com.github.bartimaeusnek.metamodjavaclientcore.dto.ServerItem;
import com.google.gson.reflect.TypeToken;
import lombok.var;
import org.eclipse.jetty.client.util.StringContentProvider;
import org.eclipse.jetty.http.HttpHeader;
import org.eclipse.jetty.http.HttpMethod;
import org.eclipse.jetty.http.HttpStatus;

import java.lang.reflect.Type;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

public class ItemClient extends AuthorizedClient {
    public ItemClient(String baseUrl, String token, String game){
    super(baseUrl, token);
        this._game = game;
    }

    private final String _game;

    /**
     * send as little requests as possible!
     * try to avoid using this inside of a loop!
     */
    public boolean postItemsAsync(ClientItem... items) throws ExecutionException, InterruptedException, TimeoutException {
        var response = getHttpClient()
                .newRequest(getUrlBase()+"Items")
                .method(HttpMethod.POST)
                .header(HttpHeader.AUTHORIZATION, "Bearer " + this.getToken())
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .content(new StringContentProvider(getGson().toJson(items)), "application/json")
                .send();

        return response.getStatus() == HttpStatus.OK_200;
    }

    /**
     * send as little requests as possible!
     * try to avoid using this inside of a loop!
     */
    public boolean requestAsync(ClientItem items) throws ExecutionException, InterruptedException, TimeoutException {
        var response = getHttpClient()
                .newRequest(getUrlBase()+"Items")
                .method(HttpMethod.PUT)
                .header(HttpHeader.AUTHORIZATION, "Bearer " + this.getToken())
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .content(new StringContentProvider(getGson().toJson(items)), "application/json")
                .send();
        var status = response.getStatus();
        return status == HttpStatus.OK_200;
    }

    public List<String> getAllItemsForGameAsync() throws ExecutionException, InterruptedException, TimeoutException {
        var response = getHttpClient()
                .newRequest(getUrlBase()+"Items/"+_game+"/Available")
                .method(HttpMethod.GET)
                .header(HttpHeader.AUTHORIZATION, "Bearer " + this.getToken())
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .send();

        Type listType = new TypeToken<List<String>>(){}.getType();
        return getGson().fromJson(response.getContentAsString(), listType);
    }

    public List<ServerItem> getAllServerItemsForUserAsync() throws ExecutionException, InterruptedException, TimeoutException {
        var response = getHttpClient()
                .newRequest(getUrlBase()+"Items/All")
                .method(HttpMethod.GET)
                .header(HttpHeader.AUTHORIZATION, "Bearer " + this.getToken())
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .send();

        Type listType = new TypeToken<List<ServerItem>>(){}.getType();
        return getGson().fromJson(response.getContentAsString(), listType);
    }

    public List<ClientItem> getAllClientItemsForUserAsync() throws ExecutionException, InterruptedException, TimeoutException {
        var response = getHttpClient()
                .newRequest(getUrlBase()+"Items/"+_game)
                .header(HttpHeader.AUTHORIZATION, "Bearer " + this.getToken())
                .method(HttpMethod.GET)
                .timeout(1, TimeUnit.MINUTES)
                .idleTimeout(1, TimeUnit.MINUTES)
                .send();

        var status = response.getStatus();
        var listType = new TypeToken<List<ClientItem>>(){}.getType();
        var content = response.getContentAsString();

        return getGson().fromJson(content, listType);
    }
}