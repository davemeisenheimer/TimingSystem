<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

    <xsl:output method="html" indent="yes"/>

    <!-- ── Utility: format milliseconds as MM:SS.hh ── -->
    <xsl:template name="formatTime">
        <xsl:param name="ms"/>
        <xsl:variable name="totalSeconds" select="floor($ms div 1000)"/>
        <xsl:variable name="minutes"      select="floor($totalSeconds div 60)"/>
        <xsl:variable name="seconds"      select="$totalSeconds mod 60"/>
        <xsl:variable name="hundredths"   select="floor($ms mod 1000 div 10)"/>
        <xsl:value-of select="format-number($minutes,    '00')"/>
        <xsl:text>:</xsl:text>
        <xsl:value-of select="format-number($seconds,    '00')"/>
        <xsl:text>.</xsl:text>
        <xsl:value-of select="format-number($hundredths, '00')"/>
    </xsl:template>

    <!-- ── Root ── -->
    <xsl:template match="/Event">
        <html>
            <head>
                <title><xsl:value-of select="EventName"/> — Results</title>
                <style>
                    body  { font-family: Arial, sans-serif; margin: 20px; color: #222; }
                    h1, h2, h3 { color: #2F4F4F; }
                    h2 { margin-top: 32px; }
                    h3 { margin-top: 24px; }
                    table { border-collapse: collapse; width: 100%; margin-bottom: 20px; }
                    th, td { border: 1px solid #ddd; padding: 8px; text-align: center; }
                    th { background-color: #e8f0fe; color: #003366; }
                    tr:nth-child(even) { background-color: #f9f9f9; }
                    td.left { text-align: left; }
                    .nickname { color: #888; font-size: 0.85em; display: block; }
                    .rank { color: #555; font-weight: bold; }
                </style>
            </head>
            <body>
                <h1><xsl:value-of select="EventName"/></h1>
                <p style="color:#666;margin-top:-12px"><xsl:value-of select="EventDate"/></p>

                <!-- ── One summary table per ResultSet ── -->
                <xsl:for-each select="ResultSets/ResultSet">
                    <h2><xsl:value-of select="@Label"/></h2>
                    <table>
                        <tr>
                            <xsl:if test="@IncludeRanking='true'">
                                <th>#</th>
                            </xsl:if>
                            <th class="left">Name</th>
                            <th>Laps</th>
                            <th>Total Time</th>
                            <th>Best Lap</th>
                            <th>Avg Lap</th>
                        </tr>
                        <xsl:for-each select="Racers/Racer">
                            <tr>
                                <xsl:if test="../../@IncludeRanking='true'">
                                    <td class="rank"><xsl:value-of select="Rank"/></td>
                                </xsl:if>
                                <td class="left">
                                    <xsl:value-of select="FirstName"/>
                                    <xsl:text> </xsl:text>
                                    <xsl:value-of select="LastName"/>
                                    <xsl:if test="NickName != ''">
                                        <span class="nickname">(<xsl:value-of select="NickName"/>)</span>
                                    </xsl:if>
                                </td>
                                <td><xsl:value-of select="LapCount"/></td>
                                <td>
                                    <xsl:call-template name="formatTime">
                                        <xsl:with-param name="ms" select="TotalTimeMs"/>
                                    </xsl:call-template>
                                </td>
                                <td>
                                    <xsl:call-template name="formatTime">
                                        <xsl:with-param name="ms" select="BestLapMs"/>
                                    </xsl:call-template>
                                </td>
                                <td>
                                    <xsl:call-template name="formatTime">
                                        <xsl:with-param name="ms" select="AvgLapMs"/>
                                    </xsl:call-template>
                                </td>
                            </tr>
                        </xsl:for-each>
                    </table>
                </xsl:for-each>

                <!-- ── Per-racer lap detail (from the first ResultSet) ── -->
                <xsl:if test="ResultSets/ResultSet[1]/Racers/Racer[1]/Laps/Lap">
                    <h2>Lap Detail</h2>
                    <xsl:for-each select="ResultSets/ResultSet[1]/Racers/Racer">
                        <h3>
                            <xsl:value-of select="FirstName"/>
                            <xsl:text> </xsl:text>
                            <xsl:value-of select="LastName"/>
                            <xsl:if test="NickName != ''">
                                <xsl:text> (</xsl:text>
                                <xsl:value-of select="NickName"/>
                                <xsl:text>)</xsl:text>
                            </xsl:if>
                        </h3>
                        <xsl:if test="Association != ''">
                            <p style="margin-top:-8px;color:#666"><xsl:value-of select="Association"/></p>
                        </xsl:if>
                        <table>
                            <tr><th>Lap</th><th>Lap Time</th></tr>
                            <xsl:for-each select="Laps/Lap">
                                <tr>
                                    <td><xsl:value-of select="LapNumber"/></td>
                                    <td>
                                        <xsl:call-template name="formatTime">
                                            <xsl:with-param name="ms" select="LapTimeMs"/>
                                        </xsl:call-template>
                                    </td>
                                </tr>
                            </xsl:for-each>
                        </table>
                    </xsl:for-each>
                </xsl:if>

            </body>
        </html>
    </xsl:template>

</xsl:stylesheet>
