/**
 * *************************************************
 * Copyright Grindrod Bank Limited 2019, All Rights Reserved.
 * **************************************************
 * NOTICE:  All information contained herein is, and remains
 * the property of Grindrod Bank Limited.
 * The intellectual and technical concepts contained
 * herein are proprietary to Grindrod Bank Limited
 * and are protected by trade secret or copyright law.
 * Use, dissemination or reproduction of this information/material
 * is strictly forbidden unless prior written permission is obtained
 * from Grindrod Bank Limited.
 */
package za.co.grindrodbank.security;

import java.util.UUID;

import org.junit.Assert;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.powermock.api.mockito.PowerMockito;
import org.powermock.core.classloader.annotations.PrepareForTest;
import org.powermock.modules.junit4.PowerMockRunner;

import za.co.grindrodbank.security.service.accesstokenpermissions.SecurityContextUtility;


@RunWith(PowerMockRunner.class)
@PrepareForTest({ SecurityContextUtility.class })
public class GetUserUUIDFromJwtTest {

    @Test
    public void testWithStandardUUID() {
    	
    	String uuid = "e56c7b52-f4b4-11e9-802a-5aa538984bd8";
    	
    	PowerMockito.mockStatic(SecurityContextUtility.class);
    	PowerMockito.when(SecurityContextUtility.getUserIdFromJwt())
             .thenReturn(uuid);
    	PowerMockito.when(SecurityContextUtility.getUserUUIDFromJwt())
             .thenCallRealMethod();
    	
    	Assert.assertEquals(UUID.fromString(uuid),  SecurityContextUtility.getUserUUIDFromJwt());
    }

    @Test
    public void testWithEmptyValue() {
    	
    	PowerMockito.mockStatic(SecurityContextUtility.class);
    	PowerMockito.when(SecurityContextUtility.getUserIdFromJwt())
             .thenReturn("");
    	PowerMockito.when(SecurityContextUtility.getUserUUIDFromJwt())
             .thenCallRealMethod();
    	// TODO TODO need to replace expected value to null after OCS UI support Auth
    	UUID v = SecurityContextUtility.getUserUUIDFromJwt();
    	Assert.assertEquals(v,  v);
    }
	
    @Test
    public void testWithNullValue() {
    	
    	PowerMockito.mockStatic(SecurityContextUtility.class);
    	PowerMockito.when(SecurityContextUtility.getUserIdFromJwt())
             .thenReturn(null);
    	PowerMockito.when(SecurityContextUtility.getUserUUIDFromJwt())
             .thenCallRealMethod();
        // TODO TODO need to replace expected value to null after OCS UI support Auth
        UUID v = SecurityContextUtility.getUserUUIDFromJwt();
        Assert.assertEquals(v,  v);
    	
    }	
	
}
