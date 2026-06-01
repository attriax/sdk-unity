package com.attriax.unity;

import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.os.IBinder;
import android.os.IInterface;
import android.os.Parcel;
import android.os.RemoteException;
import android.util.Log;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.TimeUnit;

final class AdvertisingIdProvider {
    private static final String TAG = "AttriaxAdId";
    private static final String ZERO_UUID = "00000000-0000-0000-0000-000000000000";
    private static final long BIND_TIMEOUT_MS = 1500L;

    private AdvertisingIdProvider() {
    }

    static String fetch(Context context) {
        if (context == null) {
            return null;
        }

        AdvertisingIdConnection connection = new AdvertisingIdConnection();
        Intent intent = new Intent("com.google.android.gms.ads.identifier.service.START");
        intent.setPackage("com.google.android.gms");

        boolean bound = false;
        try {
            bound = context.bindService(intent, connection, Context.BIND_AUTO_CREATE);
            if (!bound) {
                return null;
            }

            IBinder binder = connection.takeBinder();
            if (binder == null) {
                return null;
            }

            String id = readAdvertisingId(binder);
            if (id == null || id.isEmpty()) {
                return null;
            }
            if (ZERO_UUID.equalsIgnoreCase(id)) {
                return null;
            }
            if (Boolean.TRUE.equals(readLimitAdTracking(binder))) {
                return null;
            }
            return id;
        } catch (Exception exception) {
            Log.d(TAG, "GAID fetch failed: " + exception.getMessage());
            return null;
        } finally {
            if (bound) {
                try {
                    context.unbindService(connection);
                } catch (Exception ignored) {
                }
            }
        }
    }

    private static String readAdvertisingId(IBinder binder) throws RemoteException {
        Parcel data = Parcel.obtain();
        Parcel reply = Parcel.obtain();
        try {
            data.writeInterfaceToken("com.google.android.gms.ads.identifier.internal.IAdvertisingIdService");
            binder.transact(IBinder.FIRST_CALL_TRANSACTION, data, reply, 0);
            reply.readException();
            return reply.readString();
        } finally {
            reply.recycle();
            data.recycle();
        }
    }

    private static Boolean readLimitAdTracking(IBinder binder) {
        Parcel data = Parcel.obtain();
        Parcel reply = Parcel.obtain();
        try {
            data.writeInterfaceToken("com.google.android.gms.ads.identifier.internal.IAdvertisingIdService");
            data.writeInt(1);
            binder.transact(IBinder.FIRST_CALL_TRANSACTION + 1, data, reply, 0);
            reply.readException();
            return reply.readInt() != 0;
        } catch (Exception exception) {
            return null;
        } finally {
            reply.recycle();
            data.recycle();
        }
    }

    private static final class AdvertisingIdConnection implements ServiceConnection {
        private final LinkedBlockingQueue<IBinder> queue = new LinkedBlockingQueue<>(1);
        private boolean consumed = false;

        @Override
        public void onServiceConnected(ComponentName name, IBinder service) {
            queue.offer(service);
        }

        @Override
        public void onServiceDisconnected(ComponentName name) {
        }

        IBinder takeBinder() throws InterruptedException {
            if (consumed) {
                throw new IllegalStateException("binder already consumed");
            }
            consumed = true;
            return queue.poll(BIND_TIMEOUT_MS, TimeUnit.MILLISECONDS);
        }
    }

    @SuppressWarnings("unused")
    private static final Class<?> IINTERFACE = IInterface.class;
}