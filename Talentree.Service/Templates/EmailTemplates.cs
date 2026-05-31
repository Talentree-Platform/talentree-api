namespace Talentree.Service.Templates
{
    public static class EmailTemplates
    {
        public static string GetWelcomeEmail(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ 
            display: inline-block; 
            padding: 10px 20px; 
            background-color: #4CAF50; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px;
            margin-top: 15px;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Talentree! 🎉</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Thank you for registering with Talentree!</p>
            <p>We're excited to have you join our platform connecting artisan sellers with quality materials and production services.</p>
            <p>Your account has been created and is pending approval. We'll notify you once it's approved.</p>
            <a href='https://talentree.com/dashboard' class='button'>Visit Dashboard</a>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
            <p>Need help? Contact us at support@talentree.com</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetApprovalEmail(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ 
            display: inline-block; 
            padding: 10px 20px; 
            background-color: #4CAF50; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px;
            margin-top: 15px;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Account Approved! ✅</h1>
        </div>
        <div class='content'>
            <h2>Congratulations {userName}!</h2>
            <p>Your business owner account has been approved.</p>
            <p>You can now:</p>
            <ul>
                <li>Add and manage products</li>
                <li>Order raw materials</li>
                <li>Request custom production</li>
                <li>Access financial dashboard</li>
            </ul>
            <a href='https://talentree.com/login' class='button'>Login to Dashboard</a>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetRejectionEmail(string userName, string reason)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ 
            display: inline-block; 
            padding: 10px 20px; 
            background-color: #2196F3; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px;
            margin-top: 15px;
        }}
        .reason {{ 
            background-color: #fff3cd; 
            border-left: 4px solid #ffc107; 
            padding: 15px; 
            margin: 15px 0;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Application Status Update</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Thank you for your interest in joining Talentree.</p>
            <p>Unfortunately, we're unable to approve your application at this time.</p>
            <div class='reason'>
                <strong>Reason:</strong> {reason}
            </div>
            <p>If you believe this was a mistake or would like to appeal, please contact our support team.</p>
            <a href='https://talentree.com/support' class='button'>Contact Support</a>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
            <p>support@talentree.com</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetPasswordResetEmail(string userName, string resetLink)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ 
            display: inline-block; 
            padding: 10px 20px; 
            background-color: #2196F3; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px;
            margin-top: 15px;
        }}
        .warning {{ 
            background-color: #fff3cd; 
            border-left: 4px solid #ffc107; 
            padding: 15px; 
            margin: 15px 0;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>We received a request to reset your password.</p>
            <p>Click the button below to reset your password:</p>
            <a href='{resetLink}' class='button'>Reset Password</a>
            <div class='warning'>
                <strong>⚠️ Security Notice:</strong>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetOrderConfirmationEmail(string userName, string orderNumber, decimal total)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .order-box {{ 
            background-color: white; 
            border: 2px solid #4CAF50; 
            padding: 15px; 
            margin: 15px 0;
            border-radius: 5px;
        }}
        .button {{ 
            display: inline-block; 
            padding: 10px 20px; 
            background-color: #4CAF50; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px;
            margin-top: 15px;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmed! 🎉</h1>
        </div>
        <div class='content'>
            <h2>Thank you {userName}!</h2>
            <p>Your order has been confirmed and is being processed.</p>
            <div class='order-box'>
                <p><strong>Order Number:</strong> {orderNumber}</p>
                <p><strong>Total Amount:</strong> ${total:F2}</p>
            </div>
            <p>You can track your order status in your dashboard.</p>
            <a href='https://talentree.com/orders/{orderNumber}' class='button'>View Order</a>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetTicketCreatedEmail(string userName, string ticketNumber)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .ticket-box {{ 
            background-color: white; 
            border: 2px solid #2196F3; 
            padding: 15px; 
            margin: 15px 0;
            border-radius: 5px;
        }}
        .button {{ 
            display: inline-block; 
            padding: 10px 20px; 
            background-color: #2196F3; 
            color: white; 
            text-decoration: none; 
            border-radius: 5px;
            margin-top: 15px;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Support Ticket Created</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>We've received your support request and will respond shortly.</p>
            <div class='ticket-box'>
                <p><strong>Ticket Number:</strong> {ticketNumber}</p>
                <p><strong>Status:</strong> Open</p>
            </div>
            <p>Our support team typically responds within 24 hours.</p>
            <a href='https://talentree.com/support/tickets/{ticketNumber}' class='button'>View Ticket</a>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        // ✅ NEW: Email Verification OTP Template
        public static string GetEmailVerificationTemplate(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .otp-box {{ 
            background-color: #fff; 
            border: 3px dashed #4CAF50; 
            padding: 20px; 
            margin: 20px 0;
            text-align: center;
            border-radius: 10px;
        }}
        .otp-code {{ 
            font-size: 32px; 
            font-weight: bold; 
            color: #4CAF50; 
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
        }}
        .warning {{ 
            background-color: #fff3cd; 
            border-left: 4px solid #ffc107; 
            padding: 15px; 
            margin: 15px 0;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Verify Your Email</h1>
        </div>
        <div class='content'>
            <h2>Welcome to Talentree!</h2>
            <p>Thank you for registering. Please use the code below to verify your email address:</p>
            <div class='otp-box'>
                <p style='margin: 0; font-size: 14px; color: #666;'>Your Verification Code</p>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <div class='warning'>
                <strong>⚠️ Important:</strong>
                <p>• This code will expire in 10 minutes</p>
                <p>• Do not share this code with anyone</p>
                <p>• If you didn't request this, please ignore this email</p>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
            <p>Need help? Contact us at support@talentree.com</p>
        </div>
    </div>
</body>
</html>";
        }

        // ✅ NEW: Password Reset OTP Template
        public static string GetPasswordResetOtpTemplate(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .otp-box {{ 
            background-color: #fff; 
            border: 3px dashed #2196F3; 
            padding: 20px; 
            margin: 20px 0;
            text-align: center;
            border-radius: 10px;
        }}
        .otp-code {{ 
            font-size: 32px; 
            font-weight: bold; 
            color: #2196F3; 
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
        }}
        .warning {{ 
            background-color: #ffebee; 
            border-left: 4px solid #f44336; 
            padding: 15px; 
            margin: 15px 0;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Reset Your Password</h1>
        </div>
        <div class='content'>
            <h2>Password Reset Request</h2>
            <p>We received a request to reset your password. Use the code below to proceed:</p>
            <div class='otp-box'>
                <p style='margin: 0; font-size: 14px; color: #666;'>Your Reset Code</p>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <div class='warning'>
                <strong>🔒 Security Alert:</strong>
                <p>• This code will expire in 10 minutes</p>
                <p>• Never share this code with anyone, including Talentree staff</p>
                <p>• If you didn't request this, please secure your account immediately</p>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
            <p>Suspicious activity? Contact support@talentree.com</p>
        </div>
    </div>
</body>
</html>";
        }

        // ✅ NEW: Two-Factor Authentication Template
        public static string GetTwoFactorAuthTemplate(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #673AB7; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .otp-box {{ 
            background-color: #fff; 
            border: 3px dashed #673AB7; 
            padding: 20px; 
            margin: 20px 0;
            text-align: center;
            border-radius: 10px;
        }}
        .otp-code {{ 
            font-size: 32px; 
            font-weight: bold; 
            color: #673AB7; 
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
        }}
        .info {{ 
            background-color: #e3f2fd; 
            border-left: 4px solid #2196F3; 
            padding: 15px; 
            margin: 15px 0;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Two-Factor Authentication</h1>
        </div>
        <div class='content'>
            <h2>Login Verification Code</h2>
            <p>Enter this code to complete your login:</p>
            <div class='otp-box'>
                <p style='margin: 0; font-size: 14px; color: #666;'>Authentication Code</p>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <div class='info'>
                <strong>ℹ️ Security Info:</strong>
                <p>• This code expires in 5 minutes</p>
                <p>• Only enter this code on the Talentree login page</p>
                <p>• If you didn't attempt to login, change your password immediately</p>
            </div>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        // ✅ NEW: Generic OTP Template
        public static string GetGenericOtpTemplate(string otpCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #607D8B; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .otp-box {{ 
            background-color: #fff; 
            border: 3px dashed #607D8B; 
            padding: 20px; 
            margin: 20px 0;
            text-align: center;
            border-radius: 10px;
        }}
        .otp-code {{ 
            font-size: 32px; 
            font-weight: bold; 
            color: #607D8B; 
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
        }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Verification Code</h1>
        </div>
        <div class='content'>
            <h2>Your Verification Code</h2>
            <p>Please use the following code to complete your action:</p>
            <div class='otp-box'>
                <div class='otp-code'>{otpCode}</div>
            </div>
            <p style='text-align: center; color: #999; font-size: 12px;'>This code will expire in 10 minutes</p>
        </div>
        <div class='footer'>
            <p>&copy; 2026 Talentree. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        // ... keep all existing templates (GetWelcomeEmail, GetApprovalEmail, etc.) ...
    }
}