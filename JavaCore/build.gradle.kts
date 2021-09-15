@file:Suppress("GradlePackageUpdate")
plugins {
    java
    id("com.github.johnrengelman.shadow") version "7.0.0"
    id("io.freefair.lombok") version "6.2.0"
}

group = "com.github.bartimaeusnek"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

java {
    sourceCompatibility = JavaVersion.VERSION_1_8
    targetCompatibility = JavaVersion.VERSION_1_8
}

dependencies {
    implementation("com.google.code.gson:gson:2.8.8")
    implementation("org.eclipse.jetty:jetty-client:9.4.43.v20210629")
}