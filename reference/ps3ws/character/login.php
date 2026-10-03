<?php

file_put_contents("login.log", var_export($_POST, true), FILE_APPEND);

header('Content-type: text/xml');

echo '<CharacterLoginReply Status="1" StatusMessage="Test" GatewayAddress="localhost:20260" GatewayTicket="ticket" Key="key">
</CharacterLoginReply>';

?>