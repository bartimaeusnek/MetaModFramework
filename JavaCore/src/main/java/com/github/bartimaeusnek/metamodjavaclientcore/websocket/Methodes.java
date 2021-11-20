package com.github.bartimaeusnek.metamodjavaclientcore.websocket;

import lombok.Getter;

import java.nio.ByteBuffer;

public enum Methodes {
    RequestItems(0x0001),
    OverwriteData(0x0002),
    UpsertItems(0x0003),
    RequestAndDecrementItems(0x0004),
    _null(0);

    Methodes(long wsNumber) {
        this.wsNumber = ByteBuffer.allocate(8).putLong(wsNumber).array();
    }

    @Getter
    private final byte[] wsNumber;

}
