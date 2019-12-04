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
package za.co.grindrodbank.security.service.accesstokenpermissions;

import java.util.List;

import org.springframework.stereotype.Service;

@Service
public class AccessTokenPermissionsServiceImpl implements AccessTokenPermissionsService {

    public Boolean hasPermission(String permission) {
        if (permission == null) {
            return false;
        }
        
        List<String> permissions = SecurityContextUtility.getPermissionsFromJwt();

        if (permissions == null) {
            return false;
        }

        return permissions.contains(permission);
    }
}
