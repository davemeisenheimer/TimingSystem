<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="html" indent="yes"/>
	
	<xsl:template name="formatTime">
		<xsl:param name="ms" />

		<!-- total seconds -->
		<xsl:variable name="totalSeconds" select="floor($ms div 1000)"/>
		<!-- minutes -->
		<xsl:variable name="minutes" select="floor($totalSeconds div 60)"/>
		<!-- seconds (0-59) -->
		<xsl:variable name="seconds" select="$totalSeconds mod 60"/>
		<!-- remaining milliseconds -->
		<xsl:variable name="milliseconds" select="$ms mod 1000"/>

		<xsl:value-of select="format-number($minutes,'00')"/>
		<xsl:text>:</xsl:text>
		<xsl:value-of select="format-number($seconds,'00')"/>
		<xsl:text>.</xsl:text>
		<xsl:value-of select="format-number($milliseconds,'000')"/>
	</xsl:template>

	<!-- Root -->
	<xsl:template match="/Event">
		<html>
			<head>
				<title>
					<xsl:value-of select="EventName"/> Results
				</title>
				<style>
					body { font-family: Arial, sans-serif; margin: 20px; }
					h1, h2, h3 { color: #2F4F4F; }
					table { border-collapse: collapse; width: 100%; margin-bottom: 25px; }
					th, td { border: 1px solid #ddd; padding: 8px; text-align: center; }
					th { background-color: #f2f2f2; }
					tr:nth-child(even) { background-color: #f9f9f9; }
				</style>
			</head>

			<body>
				<h1>
					<xsl:value-of select="EventName"/>
				</h1>
				<h2>
					Date: <xsl:value-of select="EventDate"/>
				</h2>

				<!-- ================= Race Summary ================= -->
				<h2>Race Summary</h2>

				<table>
					<tr>
						<th>Name</th>
						<th>Total Laps</th>
						<th>Total Time</th>
						<th>Best Lap Time</th>
						<th>Average Lap Time</th>
					</tr>

					<xsl:for-each select="Racers/Racer[count(EventLaps/Lap) &gt; 0]">
						<!-- sort by lap count desc, then total time asc -->
						<xsl:sort select="count(EventLaps/Lap)" data-type="number" order="descending"/>
						<xsl:sort select="sum(EventLaps/Lap/LapTime)" data-type="number" order="ascending"/>

						<xsl:variable name="lapCount" select="count(EventLaps/Lap)"/>
						<xsl:variable name="totalTime" select="sum(EventLaps/Lap/LapTime)"/>
						<xsl:variable name="totalTimeFormatted">
								<xsl:call-template name="formatTime">
									<xsl:with-param name="ms" select="sum(EventLaps/Lap/LapTime)"/>
								</xsl:call-template>
						</xsl:variable>

						<!-- best lap = first after sorting by LapTime -->
						<xsl:variable name="bestLap">
							<xsl:for-each select="EventLaps/Lap">
								<xsl:sort select="LapTime" data-type="number" order="ascending"/>
								<xsl:if test="position() = 1">
									<xsl:call-template name="formatTime">
										<xsl:with-param name="ms" select="LapTime"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:for-each>
						</xsl:variable>

						<tr>
							<td>
								<xsl:value-of select="FirstName"/>
								<xsl:text> </xsl:text>
								<xsl:value-of select="LastName"/>
							</td>

							<td>
								<xsl:value-of select="$lapCount"/>
							</td>
							<td>
								<xsl:value-of select="$totalTimeFormatted"/>
							</td>
							<td>
								<xsl:value-of select="$bestLap"/>
							</td>
							<td>
								<xsl:call-template name="formatTime">
									<xsl:with-param name="ms" select="$totalTime div $lapCount"/>
								</xsl:call-template>
							</td>
						</tr>
					</xsl:for-each>
				</table>

				<!-- ============== Per-racer Lap Data ============== -->
				<h2>Per-racer Lap Data</h2>

				<xsl:for-each select="Racers/Racer[count(EventLaps/Lap) &gt; 0]">
					<h3>
						<xsl:value-of select="FirstName"/>
						<xsl:text> </xsl:text>
						<xsl:value-of select="LastName"/>
						<xsl:text> (</xsl:text>
						<xsl:value-of select="NickName"/>
						<xsl:text>)</xsl:text>
					</h3>

					<p>
						Association: <xsl:value-of select="Association"/>
					</p>

					<table>
						<tr>
							<th>Lap Number</th>
							<th>Lap Time</th>
						</tr>

						<xsl:for-each select="EventLaps/Lap">
							<tr>
								<td>
									<xsl:value-of select="LapNumber"/>
								</td>
								<td>
									<xsl:call-template name="formatTime">
										<xsl:with-param name="ms" select="LapTime"/>
									</xsl:call-template>
								</td>
							</tr>
						</xsl:for-each>
					</table>
				</xsl:for-each>

			</body>
		</html>
	</xsl:template>

</xsl:stylesheet>