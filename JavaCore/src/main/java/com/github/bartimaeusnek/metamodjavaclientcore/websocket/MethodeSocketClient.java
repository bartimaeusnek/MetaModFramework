package com.github.bartimaeusnek.metamodjavaclientcore.websocket;

import com.github.bartimaeusnek.metamodjavaclientcore.dto.ClientItem;
import com.github.bartimaeusnek.metamodjavaclientcore.dto.ClientItemDefinition;
import com.github.bartimaeusnek.metamodjavaclientcore.dto.ServerItem;
import com.github.bartimaeusnek.metamodjavaclientcore.dto.ServerItemDefinition;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import lombok.AccessLevel;
import lombok.Getter;
import lombok.Setter;
import lombok.var;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.drafts.Draft;
import org.java_websocket.handshake.ServerHandshake;

import java.net.URI;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.Locale;
import java.util.Map;
import java.util.function.Consumer;
import java.util.function.IntConsumer;

@Getter(AccessLevel.PROTECTED)
public class MethodeSocketClient extends WebSocketClient {

    private final Gson gson = new GsonBuilder()
            .setFieldNamingStrategy(f -> f.getName().toLowerCase(Locale.ROOT))
            .registerTypeAdapter(ClientItemDefinition.class, new ClientItemDefinition.Deserializer())
            .registerTypeAdapter(ServerItemDefinition.class, new ServerItemDefinition.Deserializer())
            .registerTypeAdapter(ClientItem.class, new ClientItem.Deserializer())
            .registerTypeAdapter(ServerItem.class, new ServerItem.Deserializer())
            .create();

    private String game;
    private byte[] game_as_Array;
    private Methodes lastSend;
    private byte[] lastBuff;
    private boolean retry = false;
    private Methodes retrySend;
    private byte[] retryBuff;

    @Getter
    @Setter
    private IntConsumer UpsertItemsCallback;
    @Getter
    @Setter
    private IntConsumer RequestAndDecrementItemsCallback;
    @Getter
    @Setter
    private Consumer<String> RequestItemsCallback;

    public MethodeSocketClient(URI serverUri, String game) {
        super(serverUri);
        this.game = game;
        this.game_as_Array = this.game.getBytes(StandardCharsets.UTF_8);
    }

    public MethodeSocketClient(URI serverUri, Draft protocolDraft, Map<String, String> httpHeaders, String game) {
        super(serverUri, protocolDraft, httpHeaders);
        this.game = game;
        this.game_as_Array = this.game.getBytes(StandardCharsets.UTF_8);
    }

    public MethodeSocketClient(URI serverUri, Draft protocolDraft, Map<String, String> httpHeaders, int connectTimeout, String game) {
        super(serverUri, protocolDraft, httpHeaders, connectTimeout);
        this.game = game;
        this.game_as_Array = this.game.getBytes(StandardCharsets.UTF_8);
    }

    @Override
    public void onOpen(ServerHandshake handshakedata) {
    }

    public void sendResyncRequest() {
        var buff =
                ByteBuffer
                        .allocate(game_as_Array.length + Methodes.RequestItems.getWsNumber().length)
                        .put(Methodes.RequestItems.getWsNumber())
                        .put(game_as_Array);

        this.lastSend = Methodes.RequestItems;
        this.lastBuff = buff.array();

        this.send(lastBuff);
    }

    @Override
    public void onMessage(ByteBuffer bytes) {
        if (bytes.get() != 1) {
            return;
        }
        this.retrySend = this.lastSend;
        this.retryBuff = this.lastBuff;
        this.retry = true;
        sendResyncRequest();
    }

    public void executeMethod(Methodes methodes, String message) {
        var prefix = methodes.getWsNumber();
        var converted = message.getBytes(StandardCharsets.UTF_8);
        var buff = ByteBuffer.allocate(prefix.length + converted.length).put(prefix).put(converted);
        this.lastSend = methodes;
        this.lastBuff = buff.array();
        this.send(lastBuff);
    }

    @Override
    public void onMessage(String message) {
        switch (lastSend) {
            case RequestItems: {
                RequestItemsCallback.accept(message);
                if (retry) {
                    this.send(retryBuff);
                    this.lastSend = this.retrySend;
                    this.retry = false;
                    this.retrySend = null;
                    this.retryBuff = null;
                }
                return;
            }
            case UpsertItems: {
                UpsertItemsCallback.accept(Integer.parseInt(message));
                return;
            }
            case RequestAndDecrementItems: {
                RequestAndDecrementItemsCallback.accept(Integer.parseInt(message));
                return;
            }
            case _null: {
                return;
            }
            case OverwriteData: {
                throw new RuntimeException("not implemented");
            }
        }
    }

    @Override
    public void onClose(int code, String reason, boolean remote) {
    }

    @Override
    public void onError(Exception ex) {
        this.close();
    }

}