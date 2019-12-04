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


import org.junit.Assert;
import org.junit.Test;

import za.co.grindrodbank.security.service.accesstokenpermissions.AccessTokenPermissionsServiceImpl;

public class AccessTokenPermissionsServiceTest {

    @Test
    public void test() {
        AccessTokenPermissionsServiceImpl accessTokenPermissionsServiceImpl = new AccessTokenPermissionsServiceImpl();
        Assert.assertFalse(accessTokenPermissionsServiceImpl.hasPermission(null));
    }

}
