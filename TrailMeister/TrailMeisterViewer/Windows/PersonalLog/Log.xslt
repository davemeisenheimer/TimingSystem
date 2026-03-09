<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:output method="html" indent="yes"/>

	<xsl:template name="formatTime">
		<xsl:param name="ms" />
		<xsl:variable name="totalSeconds" select="floor($ms div 1000)"/>
		<xsl:variable name="minutes" select="floor($totalSeconds div 60)"/>
		<xsl:variable name="seconds" select="$totalSeconds mod 60"/>
		<xsl:variable name="milliseconds" select="$ms mod 1000"/>
		<xsl:value-of select="format-number($minutes,'00')"/>
		<xsl:text>:</xsl:text>
		<xsl:value-of select="format-number($seconds,'00')"/>
		<xsl:text>.</xsl:text>
		<xsl:value-of select="format-number($milliseconds,'000')"/>
	</xsl:template>

	<xsl:template name="formatDistance">
		<xsl:param name="metres" />
		<xsl:choose>
			<xsl:when test="$metres &gt;= 1000">
				<xsl:value-of select="format-number($metres div 1000, '0.00')"/>
				<xsl:text> km</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$metres"/>
				<xsl:text> m</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Root -->
	<xsl:template match="/PersonalLog">
		<html>
			<head>
				<title>
					<xsl:value-of select="FirstName"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="LastName"/>
					<xsl:text> — Personal Log</xsl:text>
				</title>
				<style>
					body { font-family: Arial, sans-serif; margin: 20px; color: #222; }
					h1, h2, h3 { color: #2F4F4F; }
					h3 { margin-top: 32px; }
					table { border-collapse: collapse; width: 100%; margin-bottom: 25px; }
					th, td { border: 1px solid #ddd; padding: 8px; text-align: center; }
					th { background-color: #e8f0fe; color: #003366; }
					tr:nth-child(even) { background-color: #f9f9f9; }
					.subtitle { color: #666; font-size: 0.9em; margin-top: -8px; }

					.summary-row { display: flex; flex-wrap: wrap; gap: 12px; margin-bottom: 28px; }
					.summary-card { border: 1px solid #AACCEE; border-radius: 8px; background: #F0F6FF;
					                min-width: 160px; overflow: hidden; }
					.summary-card-header { background: #CCE4FF; padding: 6px 12px; text-align: center;
					                       font-weight: bold; font-size: 14px; color: #003366; }
					.summary-card table { margin: 0; border: none; width: auto; }
					.summary-card td { border: none; padding: 3px 10px; text-align: left; font-size: 13px; }
					.summary-card td.stat-label { color: #6688AA; }
					.summary-card td.stat-value { font-weight: 600; color: #112244; }
				</style>
			</head>

			<body>
				<h1>
					<xsl:value-of select="FirstName"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="LastName"/>
				</h1>
				<xsl:if test="NickName != ''">
					<p class="subtitle">
						<xsl:text>(</xsl:text>
						<xsl:value-of select="NickName"/>
						<xsl:text>)</xsl:text>
					</p>
				</xsl:if>

				<!-- ================= Season Summaries ================= -->
				<h2>Summaries</h2>
				<div class="summary-row">
					<xsl:for-each select="Summaries/Summary">
						<div class="summary-card">
							<div class="summary-card-header">
								<xsl:value-of select="Label"/>
							</div>
							<table>
								<tr>
									<td class="stat-label">Laps:</td>
									<td class="stat-value"><xsl:value-of select="LapCount"/></td>
								</tr>
								<tr>
									<td class="stat-label">Dist:</td>
									<td class="stat-value">
										<xsl:call-template name="formatDistance">
											<xsl:with-param name="metres" select="TotalDistanceMetres"/>
										</xsl:call-template>
									</td>
								</tr>
								<tr>
									<td class="stat-label">Time:</td>
									<td class="stat-value">
										<xsl:call-template name="formatTime">
											<xsl:with-param name="ms" select="TotalTimeMs"/>
										</xsl:call-template>
									</td>
								</tr>
								<tr>
									<td class="stat-label">Best:</td>
									<td class="stat-value">
										<xsl:call-template name="formatTime">
											<xsl:with-param name="ms" select="BestLapMs"/>
										</xsl:call-template>
									</td>
								</tr>
								<tr>
									<td class="stat-label">Avg:</td>
									<td class="stat-value">
										<xsl:call-template name="formatTime">
											<xsl:with-param name="ms" select="AvgLapMs"/>
										</xsl:call-template>
									</td>
								</tr>
							</table>
						</div>
					</xsl:for-each>
				</div>

				<!-- ================= Event Summary Table ================= -->
				<h2>Event Summary</h2>
				<table>
					<tr>
						<th>Event</th>
						<th>Date</th>
						<th>Laps</th>
						<th>Distance</th>
						<th>Total Time</th>
						<th>Best Lap</th>
						<th>Avg Lap</th>
					</tr>

					<xsl:for-each select="Events/Event[count(Laps/Lap) &gt; 0]">
						<xsl:variable name="lapCount" select="count(Laps/Lap)"/>
						<xsl:variable name="totalTime" select="sum(Laps/Lap/LapTime)"/>
						<xsl:variable name="totalDistance" select="sum(Laps/Lap/LapLengthMetres)"/>

						<xsl:variable name="bestLap">
							<xsl:for-each select="Laps/Lap">
								<xsl:sort select="LapTime" data-type="number" order="ascending"/>
								<xsl:if test="position() = 1">
									<xsl:call-template name="formatTime">
										<xsl:with-param name="ms" select="LapTime"/>
									</xsl:call-template>
								</xsl:if>
							</xsl:for-each>
						</xsl:variable>

						<tr>
							<td><xsl:value-of select="EventName"/></td>
							<td><xsl:value-of select="EventDate"/></td>
							<td><xsl:value-of select="$lapCount"/></td>
							<td>
								<xsl:call-template name="formatDistance">
									<xsl:with-param name="metres" select="$totalDistance"/>
								</xsl:call-template>
							</td>
							<td>
								<xsl:call-template name="formatTime">
									<xsl:with-param name="ms" select="$totalTime"/>
								</xsl:call-template>
							</td>
							<td><xsl:value-of select="$bestLap"/></td>
							<td>
								<xsl:call-template name="formatTime">
									<xsl:with-param name="ms" select="$totalTime div $lapCount"/>
								</xsl:call-template>
							</td>
						</tr>
					</xsl:for-each>
				</table>

				<!-- ================= Per-event Lap Detail ================= -->
				<h2>Lap Detail</h2>

				<xsl:for-each select="Events/Event[count(Laps/Lap) &gt; 0]">
					<h3>
						<xsl:value-of select="EventName"/>
						<xsl:text> — </xsl:text>
						<xsl:value-of select="EventDate"/>
					</h3>

					<table>
						<tr>
							<th>Lap</th>
							<th>Lap Time</th>
							<th>Distance</th>
						</tr>

						<xsl:for-each select="Laps/Lap">
							<tr>
								<td><xsl:value-of select="LapNumber"/></td>
								<td>
									<xsl:call-template name="formatTime">
										<xsl:with-param name="ms" select="LapTime"/>
									</xsl:call-template>
								</td>
								<td>
									<xsl:call-template name="formatDistance">
										<xsl:with-param name="metres" select="LapLengthMetres"/>
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
