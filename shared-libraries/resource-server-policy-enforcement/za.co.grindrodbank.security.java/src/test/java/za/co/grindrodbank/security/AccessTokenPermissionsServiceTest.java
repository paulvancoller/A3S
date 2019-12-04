/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
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
