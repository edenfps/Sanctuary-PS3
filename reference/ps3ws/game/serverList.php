<?php

file_put_contents("serverList.log", var_export($_POST, true), FILE_APPEND);

header('Content-type: text/xml');

// Statuses
// 1 - Success
// 2 - No Servers

echo '<ServerListReply Status="1">
	<StatusMessage>Test</StatusMessage>
	<Servers>
		<Server Name="Test" Online="1" Locked="0" />
	</Servers>
</ServerListReply>';

?>